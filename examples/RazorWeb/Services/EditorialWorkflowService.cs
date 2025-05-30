using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Piranha.Data.EF.SQLite; // ou o namespace correto onde definiste o contexto

namespace RazorWeb.Services
{
    public interface IEditorialWorkflowService
    {
        Task EnsurePageStatusAsync(Guid pageId);
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
    }
}
