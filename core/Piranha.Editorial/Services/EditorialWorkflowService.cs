using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto
using Piranha.Editorial.Abstractions.Enums;

namespace Piranha.Editorial.Services
{
    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId);
        Task<PageEditorialStatusDto?> GetStatusForPageAsync(Guid pageId);
        Task<bool> SubmitToEditorialReviewAsync(Guid pageId);

    }

    public class EditorialWorkflowService : IEditorialWorkflowService
    {
        private readonly ExtendedSQLiteDb _db;

        public EditorialWorkflowService(ExtendedSQLiteDb db)
        {
            _db = db;
        }

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

    }

}
