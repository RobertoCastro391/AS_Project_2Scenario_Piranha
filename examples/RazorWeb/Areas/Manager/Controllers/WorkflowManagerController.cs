using Microsoft.AspNetCore.Mvc;
using Piranha.Editorial.Repositories;

namespace RazorWeb.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class WorkflowManagerController : Controller
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowManagerController(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        [Route("manager/workflowmanager")]
        public async Task<IActionResult> Index()
        {
            var workflows = await _workflowRepository.GetAllAsync();
            return View(workflows);
        }
    }
}
