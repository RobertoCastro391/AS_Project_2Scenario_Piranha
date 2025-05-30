using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using Piranha.Editorial.ViewModels;
using Piranha.Data.EF.SQLite;

namespace RazorWeb.Services.Editorial
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ExtendedSQLiteDb _db;

        public WorkflowRepository(ExtendedSQLiteDb db)
        {
            _db = db;
        }

        public async Task<List<WorkflowListItemViewModel>> GetAllAsync()
        {
            return await _db.Workflows
                .Include(w => w.Stages)
                .Select(w => new WorkflowListItemViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    StageCount = w.Stages.Count
                })
                .ToListAsync();
        }
    }
}
