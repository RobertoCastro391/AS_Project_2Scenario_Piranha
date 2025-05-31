using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha.Editorial.ViewModels;
using Piranha.Editorial.Abstractions.Models;

namespace Piranha.Editorial.Repositories
{
    public interface IWorkflowRepository
    {
        Task<List<WorkflowListItemViewModel>> GetAllAsync();
        Task<List<PageWorkflowStatusViewModel>> GetPageWorkflowStatusesAsync();
        Task<WorkflowStage?> GetStageForPageAsync(Guid pageId);
    }
}
