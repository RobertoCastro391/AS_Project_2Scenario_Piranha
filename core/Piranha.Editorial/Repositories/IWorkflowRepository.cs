using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha.Editorial.ViewModels;

namespace Piranha.Editorial.Repositories
{
    public interface IWorkflowRepository
    {
        Task<List<WorkflowListItemViewModel>> GetAllAsync();
    }
}
