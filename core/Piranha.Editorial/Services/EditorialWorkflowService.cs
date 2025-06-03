using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto
using Piranha.Editorial.Abstractions.Enums;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;


namespace Piranha.Editorial.Services
{

    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId);
        Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId);
        Task<bool> SubmitToEditorialReviewAsync(Guid pageId);
        Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId);
        Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus);
        Task DeleteStatusForPageAsync(Guid pageId);
    }    

    public class EditorialWorkflowService : IEditorialWorkflowService
    {
        private readonly SQLiteDb _db;
        private readonly IApi _api;

        private readonly Counter<long> _transitionCounter;
        private readonly Histogram<double> _transitionDurationHistogram;
        private readonly Counter<long> _rejectedContentCounter;
        private readonly ObservableGauge<long> _pagesByStatusGauge;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Histogram<double> _timeToPublish;
        private readonly IMemoryCache _cache;


        public EditorialWorkflowService(SQLiteDb db, IApi api, Meter meter, IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            _db = db;
            _api = api;
            _scopeFactory = scopeFactory;
            _cache = cache;
            
            _transitionCounter = meter.CreateCounter<long> (
                "workflow_transition_total",
                description: "Number of transitions in editorial workflow."
            );

            _transitionDurationHistogram = meter.CreateHistogram<double>(
                "workflow_transition_duration_seconds",
                unit: "s",
                description: "Time between states transitions in the editorial workflow."
            );

            _timeToPublish = meter.CreateHistogram<double>(
                "workflow_time_to_publish_seconds",
                unit: "s",
                description: "Real Time from darft until publication."
            );

            _rejectedContentCounter = meter.CreateCounter<long>(
                "workflow_rejected_pages_total",
                description: "Total de conteúdos rejeitados no workflow editorial."
            );

            //_pagesByStatusGauge = meter.CreateObservableGauge(
            //    "workflow_pages_by_status_total",
            //    ObservePagesByStatus,
            //    description: "Number of pages currently in each editorial status.");

        }

        //private IEnumerable<Measurement<long>> ObservePagesByStatus()
        //{
        //    using var scope = _scopeFactory.CreateScope();
        //    var db = scope.ServiceProvider.GetRequiredService<SQLiteDb>();

        //    foreach (var group in db.PageEditorialStatuses
        //                 .GroupBy(p => p.Status)
        //                 .Select(g => new { Status = g.Key, Count = g.Count() }))
        //    {
        //        var label = Enum.GetName(typeof(EditorialStatus), group.Status) ?? "Unknown";
        //        yield return new Measurement<long>(group.Count, new KeyValuePair<string, object>("status", label));
        //    }

        //}


        public async Task EnsurePageStatusAsync(Guid pageId)
        {
            // Verifica se já existe estado editorial para esta página
            var exists = await _db.PageEditorialStatuses
                .AsNoTracking()
                .AnyAsync(s => s.PageId == pageId);

            if (exists)
                return;

            // Obter o workflow principal
            var workflow = await _db.Workflows
                .Include(w => w.Stages)
                .FirstOrDefaultAsync();

            if (workflow == null)
                throw new InvalidOperationException("Nenhum workflow encontrado.");

            var initialStage = workflow.Stages.OrderBy(s => s.Order).FirstOrDefault();


            if (initialStage == null)
                throw new InvalidOperationException("O workflow não tem etapa inicial definida.");

            // Criar novo estado editorial
            var state = new PageEditorialStatus
            {
                PageId = pageId,
                WorkflowId = workflow.Id,
                CurrentStageId = initialStage.Id,
                UpdatedAt = DateTime.UtcNow
            };

            _db.PageEditorialStatuses.Add(state);
            await _db.SaveChangesAsync();
        }

        public async Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId)
        {
            var status = await _db.PageEditorialStatuses
                .AsNoTracking()
                .Include(s => s.CurrentStage)
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (status == null)
                return null;

            return new PageEditorialStatusDto
            {
                Status = status.Status.ToString(),
                CurrentStageId = status.CurrentStageId,
                StageName = status.CurrentStage?.Name
            };
        }
        public async Task<bool> SubmitToEditorialReviewAsync(Guid pageId)
        {
            var pageStatus = await _db.PageEditorialStatuses
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (pageStatus == null || pageStatus.Status != EditorialStatus.Draft)
                return false;

            var stage = await _db.WorkflowStages
                .FirstOrDefaultAsync(s =>
                    s.WorkflowId == pageStatus.WorkflowId &&
                    s.Name == "Revisão Editorial");

            if (stage == null)
                return false;

            pageStatus.Status = EditorialStatus.EditorialReview;
            pageStatus.CurrentStageId = stage.Id;
            pageStatus.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId)
        {
            var pageStatus = await _db.PageEditorialStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (pageStatus == null)
                return new List<WorkflowTransition>();

            var transitions = await _db.WorkflowTransitions
                .Where(t => t.WorkflowId == pageStatus.WorkflowId && t.FromStatus == pageStatus.Status)
                .ToListAsync();

            // Define prioridade manual
            var priorities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Rejeitar"] = 0,
                ["Voltar a Rascunho"] = 1,
                ["Submeter para Revisão Editorial"] = 2,
                ["Enviar para Revisão Jurídica"] = 3,
                ["Aprovar para Publicação"] = 4,
                ["Publicar Conteúdo"] = 5
            };

            // Ordenar antes de devolver
            return transitions
                .OrderBy(t => priorities.TryGetValue(t.ActionName, out var p) ? p : 999)
                .ToList();
        }

        public async Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus)
        {
            var pageStatus = await _db.PageEditorialStatuses
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (pageStatus == null)
                return false;

            // Verifica se a transição é válida
            var valid = await _db.WorkflowTransitions
                .AnyAsync(t =>
                    t.WorkflowId == pageStatus.WorkflowId &&
                    t.FromStatus == pageStatus.Status &&
                    t.ToStatus == toStatus);

            if (!valid)
                return false;

            // Obtem a nova etapa associada ao estado destino
            var stage = await _db.WorkflowStages
                .FirstOrDefaultAsync(s => s.WorkflowId == pageStatus.WorkflowId && s.Status == toStatus);

            if (stage == null)
                return false;

            #region Metrics
            var currentStage = await _db.WorkflowStages
                .FirstOrDefaultAsync(s => s.WorkflowId == pageStatus.WorkflowId && s.Status == pageStatus.Status);

            var nextStage = stage; // já tens acima
            _transitionCounter.Add(1, new KeyValuePair<string, object>("transition", $"{currentStage?.Name}→{nextStage.Name}"));

            var last = pageStatus.UpdatedAt;
            var now = DateTime.UtcNow;
            var duration = (now - last).TotalSeconds;
            _transitionDurationHistogram.Record(duration, new("from", currentStage?.Name), new("to", nextStage.Name));

            // Só conta se a transição for rejeição para "Rascunho"
            if (nextStage.Status == EditorialStatus.Draft &&
                (currentStage.Status == EditorialStatus.EditorialReview || currentStage.Status == EditorialStatus.LegalReview))
            {
                var role = currentStage.RoleName switch
                {
                    "Editor" => "Editor",
                    "Jurista" => "Jurista",
                    _ => "Outro"
                };

                _rejectedContentCounter.Add(1,
                    new("role", role),
                    new("from", currentStage.Status.ToString()),
                    new("to", nextStage.Status.ToString()));
            }
            #endregion

            if (currentStage?.Name == "Rascunho")
            {
                _cache.Set(pageId, DateTime.UtcNow);
            }

            // Atualiza o estado editorial
            pageStatus.Status = toStatus;
            pageStatus.CurrentStageId = stage.Id;
            pageStatus.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Se for publicar a página
            if (toStatus == EditorialStatus.Published)
            {
                var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                if (page != null && page.Published == null)
                {
                    page.Published = DateTime.UtcNow;
                    await _api.Pages.SaveAsync(page);
                }

                if (_cache.TryGetValue(pageId, out DateTime startTimek))
                {
                    var totalSeconds = (DateTime.UtcNow - startTimek).TotalSeconds;
                    Console.WriteLine($"[PublishTiming] Page {pageId} → Tempo: {totalSeconds}s");

                    _timeToPublish.Record(totalSeconds, new KeyValuePair<string, object?>("pageId", pageId.ToString()));
                    _cache.Remove(pageId);
                }
                else
                {
                    Console.WriteLine($"[Warning] No draft timestamp found in cache for page {pageId} → skipping metric");
                }
            }

            // Se for voltar a rascunho, despublica
            if (toStatus == EditorialStatus.Draft)
            {
                var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                if (page != null && page.Published != null)
                {
                    page.Published = null;
                    await _api.Pages.SaveAsync(page);
                }
            }

            return true;

        }

        public async Task DeleteStatusForPageAsync(Guid pageId)
        {
            var statuses = await _db.PageEditorialStatuses
                .Where(s => s.PageId == pageId)
                .ToListAsync();

            if (statuses.Any())
            {
                _db.PageEditorialStatuses.RemoveRange(statuses);
                await _db.SaveChangesAsync();
            }
        }
    }

}
