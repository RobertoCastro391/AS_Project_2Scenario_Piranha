using Microsoft.AspNetCore.Mvc;
using RazorWeb.Services.Editorial;

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
