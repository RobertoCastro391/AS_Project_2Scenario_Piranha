using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto
using Piranha.Editorial.Abstractions.Enums;
using System.Security.Claims;

namespace Piranha.Editorial.Services
{    /// <summary>
    /// Interface for editorial workflow service.
    /// </summary>
    public interface IEditorialWorkflowService
    {
        /// <summary>
        /// Ensures a page has editorial status.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        Task EnsurePageStatusAsync(Guid pageId);
        
        /// <summary>
        /// Gets the editorial status for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>The page editorial status</returns>
        Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId);
        
        /// <summary>
        /// Submits a page to editorial review.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>True if successful</returns>
        Task<bool> SubmitToEditorialReviewAsync(Guid pageId);
        
        /// <summary>
        /// Gets available transitions for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>List of available transitions</returns>
        Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId);
        
        /// <summary>
        /// Gets available transitions for a page filtered by user permissions.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="user">The user</param>
        /// <returns>List of available transitions</returns>
        Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId, ClaimsPrincipal user);
        
        /// <summary>
        /// Applies a transition to a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="toStatus">The target status</param>
        /// <returns>True if successful</returns>
        Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus);
        
        /// <summary>
        /// Applies a transition to a page with permission checking.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="toStatus">The target status</param>
        /// <param name="user">The user</param>
        /// <returns>True if successful</returns>
        Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus, ClaimsPrincipal user);
        
        /// <summary>
        /// Deletes the editorial status for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        Task DeleteStatusForPageAsync(Guid pageId);
    }    /// <summary>
    /// Editorial workflow service implementation.
    /// </summary>
    public class EditorialWorkflowService : IEditorialWorkflowService
    {
        private readonly SQLiteDb _db;
        private readonly IApi _api;
        private readonly EditorialPermissionService _permissionService;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="db">The database context</param>
        /// <param name="api">The Piranha API</param>
        /// <param name="permissionService">The editorial permission service</param>
        public EditorialWorkflowService(SQLiteDb db, IApi api, EditorialPermissionService permissionService)
        {
            _db = db;
            _api = api;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Ensures a page has editorial status.
        /// </summary>
        /// <param name="pageId">The page ID</param>
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

        /// <summary>
        /// Gets the editorial status for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>The page editorial status</returns>
        public async Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId)
        {
            var status = await _db.PageEditorialStatuses
                .AsNoTracking()
                .Include(s => s.CurrentStage)
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (status == null)
                return null;            return new PageEditorialStatusDto
            {
                Status = status.Status.ToString(),
                CurrentStageId = status.CurrentStageId,
                StageName = status.CurrentStage?.Name ?? string.Empty
            };        }

        /// <summary>
        /// Submits a page to editorial review.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>True if successful</returns>
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

            return true;        }

        /// <summary>
        /// Gets available transitions for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>List of available transitions</returns>
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
                .ToList();        }

        /// <summary>
        /// Applies a transition to a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="toStatus">The target status</param>
        /// <returns>True if successful</returns>
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

            return true;        }

        /// <summary>
        /// Gets available transitions for a page filtered by user permissions.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="user">The user</param>
        /// <returns>List of available transitions</returns>
        public async Task<List<WorkflowTransition>> GetAvailableTransitionsAsync(Guid pageId, ClaimsPrincipal user)
        {
            var allTransitions = await GetAvailableTransitionsAsync(pageId);
            
            if (user == null)
                return new List<WorkflowTransition>();

            var allowedTransitions = new List<WorkflowTransition>();            foreach (var transition in allTransitions)
            {
                // Check if user has permission for this transition
                if (!string.IsNullOrEmpty(transition.RequiredRole))
                {
                    if (await _permissionService.CanPerformTransitionAsync(user, transition.RequiredRole))
                    {
                        allowedTransitions.Add(transition);
                    }
                }
                else
                {
                    // If no specific role required, check for general workflow access
                    if (await _permissionService.HasWorkflowAccessAsync(user))
                    {
                        allowedTransitions.Add(transition);
                    }
                }
            }

            return allowedTransitions;
        }

        /// <summary>
        /// Applies a transition to a page with permission checking.
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <param name="toStatus">The target status</param>
        /// <param name="user">The user</param>
        /// <returns>True if successful</returns>
        public async Task<bool> ApplyTransitionAsync(Guid pageId, EditorialStatus toStatus, ClaimsPrincipal user)
        {
            if (user == null)
                return false;

            var pageStatus = await _db.PageEditorialStatuses
                .FirstOrDefaultAsync(s => s.PageId == pageId);

            if (pageStatus == null)
                return false;

            // Get the specific transition being requested
            var transition = await _db.WorkflowTransitions
                .FirstOrDefaultAsync(t =>
                    t.WorkflowId == pageStatus.WorkflowId &&
                    t.FromStatus == pageStatus.Status &&
                    t.ToStatus == toStatus);

            if (transition == null)
                return false;            // Check permissions for this transition
            if (!string.IsNullOrEmpty(transition.RequiredRole))
            {
                if (!await _permissionService.CanPerformTransitionAsync(user, transition.RequiredRole))
                    return false;
            }
            else
            {
                // If no specific role required, check for general workflow access
                if (!await _permissionService.HasWorkflowAccessAsync(user))
                    return false;
            }

            // If permission check passes, apply the transition
            return await ApplyTransitionAsync(pageId, toStatus);
        }

        /// <summary>
        /// Deletes the editorial status for a page.
        /// </summary>
        /// <param name="pageId">The page ID</param>
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
