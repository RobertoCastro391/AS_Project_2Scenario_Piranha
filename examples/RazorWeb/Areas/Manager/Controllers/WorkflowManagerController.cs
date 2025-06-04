using Microsoft.AspNetCore.Mvc;
using Piranha.Editorial.Repositories;
using Piranha.Editorial.ViewModels;
using Piranha.Data.EF.SQLite;
using System.Diagnostics.Metrics;


namespace RazorWeb.Areas.Manager.Controllers
{
    [Area("Manager")]    public class WorkflowManagerController : Controller
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly SQLiteDb _db;
        private readonly Counter<long> _pageVisitCounter;

        public WorkflowManagerController(IWorkflowRepository workflowRepository, SQLiteDb db, Meter meter)
        {
            _workflowRepository = workflowRepository;
            _db = db;
            _pageVisitCounter = meter.CreateCounter<long>("workflow", description: "Workflow Manager count visits.");
        }

        [Route("manager/workflowmanager")]
        public async Task<IActionResult> Index()
        {

            var pages = await _workflowRepository.GetPageWorkflowStatusesAsync();
            _pageVisitCounter.Add(1, KeyValuePair.Create<string, object?>("area", "manager"));
            return View(pages);
        }
    }
}