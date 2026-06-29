using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;
using TestAutomationPlatform.Services;

namespace TestAutomationPlatform.Controllers
{
    public class ScriptsFormController : Controller
    {
        private readonly AppDbContext _context;
        private readonly RunService _runService;

        public ScriptsFormController(AppDbContext context, RunService runService)
        {
            _context = context;
            _runService = runService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? testSuiteId)
        {
            var scriptsQuery = _context.Scripts
                .Include(s => s.Workspace)
                .Include(s => s.Category)
                    .ThenInclude(c => c.Workspace)
                .Include(s => s.TestSuite)
                    .ThenInclude(ts => ts.Category)
                        .ThenInclude(c => c.Workspace)
                .AsQueryable();

            if (testSuiteId.HasValue)
            {
                var testSuite = await _context.TestSuites
                    .Include(ts => ts.Category)
                        .ThenInclude(c => c.Workspace)
                    .Include(ts => ts.Workspace)
                    .FirstOrDefaultAsync(ts => ts.Id == testSuiteId.Value);

                if (testSuite == null)
                {
                    return NotFound();
                }

                scriptsQuery = scriptsQuery
                    .Where(s => s.TestSuiteId == testSuiteId.Value);

                ViewBag.IsSuiteView = true;
                ViewBag.TestSuiteId = testSuite.Id;
                ViewBag.TestSuiteName = testSuite.Name;
                ViewBag.CategoryName = testSuite.Category?.Name ?? "Geen categorie";
                ViewBag.WorkspaceName =
                    testSuite.Category?.Workspace?.Name
                    ?? testSuite.Workspace?.Name
                    ?? "Geen workspace";
            }
            else
            {
                ViewBag.IsSuiteView = false;
            }

            var scripts = await scriptsQuery
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(scripts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? testSuiteId)
        {
            if (!testSuiteId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Maak een script aan vanuit een test-suite.";

                return RedirectToAction("Index", "TestStructure");
            }

            var testSuite = await _context.TestSuites
                .Include(ts => ts.Category)
                    .ThenInclude(c => c.Workspace)
                .Include(ts => ts.Workspace)
                .FirstOrDefaultAsync(ts => ts.Id == testSuiteId.Value);

            if (testSuite == null)
            {
                return NotFound();
            }

            if (testSuite.Category == null ||
                !testSuite.CategoryId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Deze test-suite is nog niet aan een categorie gekoppeld.";

                return RedirectToAction(
                    "Index",
                    "TestStructure",
                    new { workspaceId = testSuite.WorkspaceId });
            }

            ViewBag.TestSuiteName = testSuite.Name;
            ViewBag.CategoryName = testSuite.Category.Name;
            ViewBag.WorkspaceName = testSuite.Category.Workspace.Name;

            var viewModel = new ScriptFormViewModel
            {
                TestSuiteId = testSuite.Id,
                CategoryId = testSuite.CategoryId.Value,
                WorkspaceId = testSuite.Category.WorkspaceId
            };

            return View(await BuildViewModel(viewModel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScriptFormViewModel viewModel)
        {
            var selectedTestSuite = await _context.TestSuites
                .Include(ts => ts.Category)
                    .ThenInclude(c => c.Workspace)
                .Include(ts => ts.Workspace)
                .FirstOrDefaultAsync(ts => ts.Id == viewModel.TestSuiteId);

            if (selectedTestSuite == null)
            {
                TempData["ErrorMessage"] =
                    "De gekozen test-suite bestaat niet.";

                return RedirectToAction("Index", "TestStructure");
            }

            if (selectedTestSuite.Category == null ||
                !selectedTestSuite.CategoryId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "Deze test-suite is nog niet aan een categorie gekoppeld.";

                return RedirectToAction(
                    "Index",
                    "TestStructure",
                    new { workspaceId = selectedTestSuite.WorkspaceId });
            }

            viewModel.WorkspaceId = selectedTestSuite.Category.WorkspaceId;
            viewModel.CategoryId = selectedTestSuite.CategoryId.Value;
            viewModel.TestSuiteId = selectedTestSuite.Id;

            ViewBag.TestSuiteName = selectedTestSuite.Name;
            ViewBag.CategoryName = selectedTestSuite.Category.Name;
            ViewBag.WorkspaceName = selectedTestSuite.Category.Workspace.Name;

            if (!ModelState.IsValid)
            {
                return View(await BuildViewModel(viewModel));
            }

            var script = new Script
            {
                Name = viewModel.Name,
                Code = viewModel.Code,
                CreatedAt = DateTime.Now,

                TestSuiteId = selectedTestSuite.Id,

                // Tijdelijk nog nodig zolang deze oude kolommen bestaan.
                CategoryId = selectedTestSuite.CategoryId.Value,
                WorkspaceId = selectedTestSuite.Category.WorkspaceId
            };

            _context.Scripts.Add(script);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Script is succesvol aangemaakt.";

            return RedirectToAction(
                nameof(Index),
                new { testSuiteId = selectedTestSuite.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var script = await _context.Scripts.FindAsync(id);

            if (script == null)
            {
                return NotFound();
            }

            var vm = await BuildViewModel();

            vm.Id = script.Id;
            vm.Name = script.Name;
            vm.Code = script.Code;
            vm.WorkspaceId = script.WorkspaceId;
            vm.TestSuiteId = script.TestSuiteId;
            vm.CategoryId = script.CategoryId;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ScriptFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(await BuildViewModel(viewModel));
            }

            var script = await _context.Scripts.FindAsync(viewModel.Id);

            if (script == null)
            {
                return NotFound();
            }

            script.Name = viewModel.Name;
            script.Code = viewModel.Code;
            script.WorkspaceId = viewModel.WorkspaceId;
            script.TestSuiteId = viewModel.TestSuiteId;
            script.CategoryId = viewModel.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index),
                new { testSuiteId = script.TestSuiteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? testSuiteId)
        {
            var script = await _context.Scripts.FindAsync(id);

            if (script == null)
            {
                TempData["ErrorMessage"] = "Script niet gevonden.";

                return RedirectToAction(
                    nameof(Index),
                    testSuiteId.HasValue
                        ? new { testSuiteId }
                        : null);
            }

            var hasScheduledRuns = await _context.ScheduledRuns
                .AnyAsync(sr => sr.ScriptId == id);

            if (hasScheduledRuns)
            {
                TempData["ErrorMessage"] =
                    "Dit script kan niet worden verwijderd omdat er nog een planning aan gekoppeld is. Verwijder eerst de planning.";

                return RedirectToAction(
                    nameof(Index),
                    testSuiteId.HasValue
                        ? new { testSuiteId }
                        : null);
            }

            _context.Scripts.Remove(script);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Script is verwijderd.";

            return RedirectToAction(
                nameof(Index),
                testSuiteId.HasValue
                    ? new { testSuiteId }
                    : null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id, int? testSuiteId)
        {
            var script = await _context.Scripts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (script == null)
            {
                TempData["ErrorMessage"] = "Script niet gevonden.";

                return RedirectToAction(
                    nameof(Index),
                    testSuiteId.HasValue
                        ? new { testSuiteId }
                        : null);
            }

            await _runService.ExecuteRunByScriptId(id);

            TempData["Message"] = $"Script {id} uitgevoerd.";

            return RedirectToAction(
                nameof(Index),
                testSuiteId.HasValue
                    ? new { testSuiteId }
                    : null);
        }

        private async Task<ScriptFormViewModel> BuildViewModel(
            ScriptFormViewModel vm = null)
        {
            vm ??= new ScriptFormViewModel();

            vm.Workspaces = await _context.Workspaces
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name
                })
                .ToListAsync();

            vm.TestSuites = await _context.TestSuites
                .Select(ts => new SelectListItem
                {
                    Value = ts.Id.ToString(),
                    Text = ts.Name
                })
                .ToListAsync();

            vm.Categories = await _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            return vm;
        }
    }
}