using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Piranha.Editorial.Repositories;
using Piranha.Editorial.ViewModels;
using Piranha.Editorial.Abstractions.Models;
using Piranha.Data.EF.SQLite;
using Piranha.AspNetCore.Identity.Data;
using Piranha.AspNetCore.Identity.SQLite;

using System.Diagnostics.Metrics;


namespace RazorWeb.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class WorkflowManagerController : Controller
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly SQLiteDb _db;
         private readonly Counter<long> _pageVisitCounter;
        private readonly IdentitySQLiteDb _identityDb;
        private readonly UserManager<User> _userManager;

        public WorkflowManagerController(
            IWorkflowRepository workflowRepository,
            SQLiteDb db,
            IdentitySQLiteDb identityDb,
            UserManager<User> userManager,, Meter meter)
        {
            _workflowRepository = workflowRepository;
            _db = db;
            _identityDb = identityDb;
            _userManager = userManager;
             _pageVisitCounter = meter.CreateCounter<long>("workflow", description: "Workflow Manager count visits.");

      
        }

        [Route("manager/workflowmanager")]
        public async Task<IActionResult> Index()
        {


            var pages = await _workflowRepository.GetPageWorkflowStatusesAsync();
            // Carrega todos os históricos
            var allHistories = await _db.ContentStateHistories.ToListAsync();

            // Dicionário de ID → Nome do utilizador (a partir do contexto de Identity)
            var userNames = await _identityDb.Users
                .ToDictionaryAsync(u => u.Id, u => u.UserName);

            foreach (var page in pages)
            {
                var pageId = page.PageId;

                var pageHistory = allHistories
                    .Where(h => h.ContentId == pageId)
                    .OrderByDescending(h => h.Timestamp)
                    .Select(h => new ContentStateHistory
                    {
                        Id = h.Id,
                        ContentId = h.ContentId,
                        FromStatus = h.FromStatus,
                        ToStatus = h.ToStatus,
                        Action = h.Action,
                        Comment = h.Comment,
                        UserId = Guid.TryParse(h.UserId, out var guid) && userNames.TryGetValue(guid, out var name)
                            ? name
                            : "Desconhecido",
                        Timestamp = h.Timestamp
                    })
                    .ToList();

                page.History = pageHistory;
            }


            _pageVisitCounter.Add(1, KeyValuePair.Create<string, object?>("area", "manager"));

            return View(pages);
        }
    }
}