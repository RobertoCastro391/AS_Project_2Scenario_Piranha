using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto
using Piranha.Editorial.Abstractions.Enums;
using System.Security.Claims;


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


    }    public class EditorialWorkflowService : IEditorialWorkflowService
    {
        private readonly SQLiteDb _db;
        private readonly IApi _api;



        public EditorialWorkflowService(SQLiteDb db, IApi api)
        {
            _db = db;
            _api = api;
        }

        public async Task EnsurePageStatusAsync(Guid pageId,string userId)
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
            var pageStatus = await _db.PageEditorialStatuses
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (pageStatus == null)
                return false;

            var fromStatus = pageStatus.Status;

            var transition = await _db.WorkflowTransitions
                    .FirstOrDefaultAsync(t =>
            t.WorkflowId == pageStatus.WorkflowId &&
            t.FromStatus == fromStatus &&
            t.ToStatus == toStatus);

            if (transition == null)
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

            // Atualiza o estado editorial
            pageStatus.Status = toStatus;
            pageStatus.CurrentStageId = stage.Id;
            pageStatus.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            

            // 🔵 Se for publicar a página
            if (toStatus == EditorialStatus.Published)
            {
                var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                if (page != null && page.Published == null)
                {
                    page.Published = DateTime.UtcNow;
                    await _api.Pages.SaveAsync(page);
                }
            }

            // 🔴 Se for voltar a rascunho, despublica
            if (toStatus == EditorialStatus.Draft)
            {
                var page = await _api.Pages.GetByIdAsync<Piranha.Models.PageBase>(pageId);
                if (page != null && page.Published != null)
                {
                    page.Published = null;
                    await _api.Pages.SaveAsync(page);
                }
            }



            await LogTransitionAsync(pageId, fromStatus, toStatus, transition.ActionName, userId);

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
