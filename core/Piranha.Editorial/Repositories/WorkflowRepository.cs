using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Abstractions.Models;
using Piranha.Editorial.ViewModels;
using Piranha.Data.EF.SQLite;
using System.Collections.Generic;
using Piranha.Models;

namespace Piranha.Editorial.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ExtendedSQLiteDb _db;
        private readonly IApi _api;

        public WorkflowRepository(ExtendedSQLiteDb db, IApi api)
        {
            _db = db;
            _api = api;
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

        public async Task<List<PageWorkflowStatusViewModel>> GetPageWorkflowStatusesAsync()
        {
            var allPages = new List<PageBase>();
            var sites = await _api.Sites.GetAllAsync();
            var siteDict = sites.ToDictionary(s => s.Id, s => s.Title);

            foreach (var site in sites)
            {
                var sitePages = await _api.Pages.GetAllAsync<PageBase>(site.Id);
                allPages.AddRange(sitePages);
            }

            var stages = await _db.WorkflowStages.ToListAsync();
            var statuses = await _db.PageEditorialStatuses.ToListAsync();

            var list = new List<PageWorkflowStatusViewModel>();

            foreach (var page in allPages)
            {
                var status = statuses.FirstOrDefault(s => s.PageId == page.Id);
                var stage = status != null ? stages.FirstOrDefault(st => st.Id == status.CurrentStageId) : null;

                list.Add(new PageWorkflowStatusViewModel
                {
                    PageId = page.Id,
                    Title = page.Title,
                    Created = page.Created,
                    StatusCMS = page.Published.HasValue ? "Published" : "Draft",
                    WorkflowStage = stage?.Name ?? "Sem estado",
                    SiteName = siteDict.ContainsKey(page.SiteId) ? siteDict[page.SiteId] : "Desconhecido"
                });
            }

            return list;
        }

        public async Task<WorkflowStage?> GetStageForPageAsync(Guid pageId)
        {
            var status = await _db.PageEditorialStatuses
                .Where(p => p.PageId == pageId)
                .FirstOrDefaultAsync();

            if (status == null)
                return null;

            return await _db.WorkflowStages
                .FirstOrDefaultAsync(s => s.Id == status.CurrentStageId);
        }
    }
}
