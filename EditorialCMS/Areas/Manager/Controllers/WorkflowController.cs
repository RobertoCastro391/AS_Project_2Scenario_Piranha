using Microsoft.AspNetCore.Mvc;
using Editorial.Workflows.Services;
using Piranha;
using Piranha.Models;
using Editorial.Workflows.Data;
using Microsoft.EntityFrameworkCore;
using EditorialCMS.Models;
using EditorialCMS.Services;

namespace EditorialCMS.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class WorkflowController : Controller
    {
        private readonly IApi _api;
        private readonly IWorkflowService _workflowService;
        private readonly WorkflowDbContext _db;

        public WorkflowController(IApi api, IWorkflowService workflowService, WorkflowDbContext db)
        {
            _api = api;
            _workflowService = workflowService;
            _db = db;
        }

        public async Task<IActionResult> CreatePage()
        {
            var workflows = await _db.Workflows.ToListAsync();
            ViewBag.Workflows = workflows;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePage(string title, Guid workflowId)
        {
            var page = await StandardPage.CreateAsync(_api);
            page.Title = title;
            page.SiteId = (await _api.Sites.GetDefaultAsync()).Id;
            page.Slug = Utils.GenerateSlug(title);
            page.NavigationTitle = title;

            await _api.Pages.SaveAsync(page);
            await _workflowService.BindContentToWorkflowAsync("Page", page.Id.ToString(), workflowId);

            TempData["Success"] = "Página criada com sucesso e associada ao workflow!";
            return RedirectToAction("CreatePage");
        }

        [HttpGet]
        public async Task<IActionResult> EditPage(Guid id)
        {
            var page = await _api.Pages.GetByIdAsync<StandardPage>(id);

            if (page == null)
                return NotFound();

            var binding = await _db.ContentBindings
                .Include(b => b.CurrentStage)
                .FirstOrDefaultAsync(b => b.ContentId == id.ToString());

            if (binding == null)
                return NotFound("Binding de workflow não encontrado para esta página.");

            ViewBag.Stage = binding.CurrentStage.Name;

            return View(page);
        }

        [HttpPost]
        public async Task<IActionResult> EditPage(Guid id, string title)
        {
            var page = await _api.Pages.GetByIdAsync<StandardPage>(id);

            if (page == null)
                return NotFound();

            page.Title = title;
            page.NavigationTitle = title;
            page.Slug = Utils.GenerateSlug(title);

            await _api.Pages.SaveAsync(page);

            TempData["Success"] = "Página atualizada com sucesso!";
            return RedirectToAction("EditPage", new { id = page.Id });
        }
    }
}
