using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto
using Piranha.Editorial.Abstractions.Enums;
using System.Security.Claims;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace Piranha.Editorial.Services
{

    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId, string userId);
        Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId);
        Task<bool> SubmitToEditorialReviewAsync(Guid pageId);
        Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId);
        Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus, string userId);

        Task DeleteStatusForPageAsync(Guid pageId);
        Task<List<WorkflowTransition>> GetTransitionsForRolesAsync(Guid pageId, List<string> userRoles);


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
            private readonly ILogger<EditorialWorkflowService> _logger;

            private static readonly ActivitySource _activitySource = new("RazorWeb.Service");


            public EditorialWorkflowService(SQLiteDb db, IApi api, Meter meter, IServiceScopeFactory scopeFactory, IMemoryCache cache, ILogger<EditorialWorkflowService> logger)

            {
                _db = db;
                _api = api;
                _scopeFactory = scopeFactory;
                _cache = cache;
                _logger = logger;

                _transitionCounter = meter.CreateCounter<long>(
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

            public async Task EnsurePageStatusAsync(Guid pageId, string userId)
            {
                using var activity = _activitySource.StartActivity("EnsurePageStatus", ActivityKind.Internal);
                activity?.SetTag("page.id", pageId.ToString());

                try
                {
                    // Verifica se já existe estado editorial para esta página
                    var exists = await _db.PageEditorialStatuses
                    .AsNoTracking()
                    .AnyAsync(s => s.PageId == pageId);

                    activity?.SetTag("alreadyExists", exists);

                    if (exists)
                    {
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return;
                    }

                    // Obter o workflow principal
                    var workflow = await _db.Workflows
                        .Include(w => w.Stages)
                        .FirstOrDefaultAsync();

                    if (workflow == null)
                        throw new InvalidOperationException("Nenhum workflow encontrado.");

                    var initialStage = workflow.Stages.OrderBy(s => s.Order).FirstOrDefault();


                    if (initialStage == null)
                        throw new InvalidOperationException("O workflow não tem etapa inicial definida.");

                    activity?.SetTag("workflow.id", workflow.Id.ToString());
                    activity?.SetTag("stage.id", initialStage.Id.ToString());
                    var state = new PageEditorialStatus
                    {
                        PageId = pageId,
                        WorkflowId = workflow.Id,
                        CurrentStageId = initialStage.Id,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _db.PageEditorialStatuses.Add(state);

                    var hasHistory = await _db.ContentStateHistories
                            .AnyAsync(h => h.ContentId == pageId);

                    if (!hasHistory)
                    {
                        _db.ContentStateHistories.Add(new ContentStateHistory
                        {
                            Id = Guid.NewGuid(),
                            ContentId = pageId,
                            FromStatus = EditorialStatus.Draft,
                            ToStatus = EditorialStatus.Draft,
                            Action = "Criação Inicial",
                            Comment = null,
                            UserId = userId ?? "anonymous",
                            Timestamp = DateTime.UtcNow
                        });

                        await _db.SaveChangesAsync();
                    }
                    await _db.SaveChangesAsync();

                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.SetTag("exception", ex.ToString());
                    throw;
                }

            }

            public async Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId)
            {
                using var activity = _activitySource.StartActivity("GetPageEditorialStatus", ActivityKind.Internal);
                activity?.SetTag("page.id", pageId.ToString());

                _logger.LogInformation("Getting editorial for a página {PageId}", pageId);

                try
                {
                    var status = await _db.PageEditorialStatuses
                        .AsNoTracking()
                        .Include(s => s.CurrentStage)
                        .FirstOrDefaultAsync(s => s.PageId == pageId);

                    if (status == null)
                    {
                        _logger.LogWarning("Nenhum status editorial encontrado para a página {PageId}", pageId);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("status.found", false);
                        return null;
                    }

                    _logger.LogInformation("Status atual da página {PageId} é {Status} na etapa {Stage}", pageId, status.Status, status.CurrentStage?.Name);

                    activity?.SetTag("status.found", true);
                    activity?.SetTag("status.stage", status.CurrentStage?.Name ?? "unknown");
                    activity?.SetTag("status.value", status.Status.ToString());

                    return new PageEditorialStatusDto
                    {
                        Status = status.Status.ToString(),
                        CurrentStageId = status.CurrentStageId,
                        StageName = status.CurrentStage?.Name
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting editorial status for page {PageId}", pageId);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.SetTag("exception", ex.ToString());
                    throw;
                }
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

                _logger.LogWarning("Submiting from Draft to Editorial Review for page: {PageId}", pageId);

                pageStatus.Status = EditorialStatus.EditorialReview;
                pageStatus.CurrentStageId = stage.Id;
                pageStatus.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return true;
            }
            public async Task<List<WorkflowTransition>> GetTransitionsForRolesAsync(Guid pageId, List<string> userRoles)
            {
                var pageStatus = await _db.PageEditorialStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.PageId == pageId);

                if (pageStatus == null)
                    return new List<WorkflowTransition>();

                var allTransitions = await _db.WorkflowTransitions
                    .Where(t => t.WorkflowId == pageStatus.WorkflowId && t.FromStatus == pageStatus.Status)
                    .ToListAsync();

                var priorities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Rejeitar"] = 0,
                    ["Voltar a Rascunho"] = 1,
                    ["Submeter para Revisão Editorial"] = 2,
                    ["Enviar para Revisão Jurídica"] = 3,
                    ["Aprovar para Publicação"] = 4,
                    ["Publicar Conteúdo"] = 5
                };

                var filtered = allTransitions
                    .Where(t =>
                        userRoles.Contains(t.RequiredRole.ToLowerInvariant()) ||
                        userRoles.Contains("sysadmin") ||
                        userRoles.Contains("diretor"))
                    .OrderBy(t => priorities.TryGetValue(t.ActionName, out var p) ? p : 999)
                    .GroupBy(t => t.ToStatus)
                    .Select(g => g.First()) // só uma transição por destino
                    .ToList();

                return filtered;
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
                        .GroupBy(t => t.ToStatus)
                        .Select(g => g.First())
                        .ToList();

            }

            public async Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus, string userId)
            {
                using var activity = _activitySource.StartActivity("ApplyWorkflowTransition", ActivityKind.Internal);
                activity?.SetTag("page.id", pageId.ToString());
                activity?.SetTag("target.status", toStatus.ToString());

                _logger.LogInformation("Applying transition to status '{TargetStatus}' for page {PageId}", toStatus, pageId);

                try
                {
                    var pageStatus = await _db.PageEditorialStatuses.FirstOrDefaultAsync(s => s.PageId == pageId);
                    if (pageStatus == null)
                    {
                        _logger.LogWarning("No editorial status found for page {PageId}", pageId);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return false;
                    }

                    var fromStatus = pageStatus.Status;

                    // Verifica se a transição é válida
                    var transition = await _db.WorkflowTransitions
                        .FirstOrDefaultAsync(t =>
                            t.WorkflowId == pageStatus.WorkflowId &&
                            t.FromStatus == fromStatus &&
                            t.ToStatus == toStatus);

                    if (transition == null)
                    {
                        _logger.LogWarning("Invalid transition from {From} to {To} for page {PageId}", fromStatus, toStatus, pageId);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return false;
                    }

                    // Obtem o stage destino
                    var stage = await _db.WorkflowStages
                        .FirstOrDefaultAsync(s => s.WorkflowId == pageStatus.WorkflowId && s.Status == toStatus);

                    if (stage == null)
                    {
                        _logger.LogError("Target stage not found for status {Status} in workflow {WorkflowId}", toStatus, pageStatus.WorkflowId);
                        activity?.SetStatus(ActivityStatusCode.Error, "Target stage not found");
                        return false;
                    }

                    // Métricas
                    var currentStage = await _db.WorkflowStages
                        .FirstOrDefaultAsync(s => s.WorkflowId == pageStatus.WorkflowId && s.Status == fromStatus);

                    var duration = (DateTime.UtcNow - pageStatus.UpdatedAt).TotalSeconds;
                    _transitionCounter.Add(1, new KeyValuePair<string, object?>[] { new("transition", $"{currentStage?.Name}→{stage.Name}") });
                    _transitionDurationHistogram.Record(duration, new("from", currentStage?.Name), new("to", stage.Name));
                    activity?.SetTag("duration.seconds", duration);

                    // Contagem de rejeições
                    if (stage.Status == EditorialStatus.Draft &&
                        (currentStage?.Status == EditorialStatus.EditorialReview || currentStage?.Status == EditorialStatus.LegalReview))
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
                            new("to", stage.Status.ToString()));

                        _logger.LogInformation("Page {PageId} rejected by {Role}, returned to draft", pageId, role);
                    }

                    // Guarda tempo de entrada no rascunho (para depois calcular time-to-publish)
                    if (currentStage?.Name == "Rascunho")
                        _cache.Set(pageId, DateTime.UtcNow);

                    // Atualiza o estado editorial
                    pageStatus.Status = toStatus;
                    pageStatus.CurrentStageId = stage.Id;
                    pageStatus.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    // Publicar a página
                    if (toStatus == EditorialStatus.Published)
                    {
                        var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                        if (page != null && page.Published == null)
                        {
                            page.Published = DateTime.UtcNow;
                            await _api.Pages.SaveAsync(page);

                            if (_cache.TryGetValue(pageId, out DateTime draftTime))
                            {
                                var totalSeconds = (DateTime.UtcNow - draftTime).TotalSeconds;
                                _timeToPublish.Record(totalSeconds, new KeyValuePair<string, object?>[] { new("pageId", pageId.ToString()) });
                                _cache.Remove(pageId);

                                _logger.LogInformation("Page {PageId} published after {Seconds} seconds", pageId, totalSeconds);
                            }
                            else
                            {
                                _logger.LogWarning("No cached draft time for page {PageId}. Time-to-publish metric skipped.", pageId);
                            }
                        }
                    }

                    // Voltar a rascunho → despublicar
                    if (toStatus == EditorialStatus.Draft)
                    {
                        var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                        if (page != null && page.Published != null)
                        {
                            page.Published = null;
                            await _api.Pages.SaveAsync(page);
                            _logger.LogInformation("Page {PageId} unpublished after returning to draft.", pageId);
                        }
                    }

                    _logger.LogInformation("Successfully transitioned page {PageId} to status {Status}", pageId, toStatus);
                    activity?.SetStatus(ActivityStatusCode.Ok);

                    await LogTransitionAsync(pageId, fromStatus, toStatus, transition.ActionName, userId);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during transition for page {PageId}", pageId);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.SetTag("exception", ex.ToString());
                    throw;
                }
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



            private async Task LogTransitionAsync(Guid pageId, EditorialStatus fromStatus, EditorialStatus toStatus, string action, string userId, string? comment = null)
            {
                var log = new ContentStateHistory
                {
                    Id = Guid.NewGuid(),
                    ContentId = pageId,
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    Action = action,
                    Comment = comment,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                _db.ContentStateHistories.Add(log);
                await _db.SaveChangesAsync();
            }


        }
    }
}
