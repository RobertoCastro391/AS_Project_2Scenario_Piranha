using Microsoft.AspNetCore.Mvc;
using Piranha.Editorial.Repositories;
using Piranha.Editorial.ViewModels;
using Piranha.Data.EF.SQLite;

namespace RazorWeb.Areas.Manager.Controllers
{
    [Area("Manager")]    public class WorkflowManagerController : Controller
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly SQLiteDb _db;

        public WorkflowManagerController(IWorkflowRepository workflowRepository, SQLiteDb db)
        {
            _workflowRepository = workflowRepository;
            _db = db;
        }

        [Route("manager/workflowmanager")]
        public async Task<IActionResult> Index()
        {
            var pages = await _workflowRepository.GetPageWorkflowStatusesAsync();

            return View(pages);
        }
    }
}
