using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha.Editorial.ViewModels;

namespace RazorWeb.Services.Editorial
{
    public interface IWorkflowRepository
    {
        Task<List<WorkflowListItemViewModel>> GetAllAsync();
    }
}
