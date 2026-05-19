using Hangfire;
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
        private readonly ScriptParser _scriptParser;
        private readonly IBackgroundJobClient _backgroundJobs;

        public ScriptsFormController(AppDbContext context, ScriptParser scriptParser, IBackgroundJobClient backgroundJobs)
        {
            _context = context;
            _scriptParser = scriptParser;
            _backgroundJobs = backgroundJobs;
        }

        public async Task<IActionResult> Index()
        {
            var scripts = await _context.Scripts
                .Include(s => s.Workspace)
                .Include(s => s.TestSuite)
                .Include(s => s.Category)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(scripts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(await BuildViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScriptFormViewModel viewModel)
        {
            AddScriptValidationErrors(viewModel.Code);

            if (!ModelState.IsValid)
                return View(await BuildViewModel(viewModel));

            var script = new Script
            {
                Name = viewModel.Name,
                Code = viewModel.Code,
                WorkspaceId = viewModel.WorkspaceId,
                TestSuiteId = viewModel.TestSuiteId,
                CategoryId = viewModel.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Scripts.Add(script);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Script '{script.Name}' opgeslagen.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var script = await _context.Scripts.FindAsync(id);

            if (script == null)
                return NotFound();

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
            AddScriptValidationErrors(viewModel.Code);

            if (!ModelState.IsValid)
                return View(await BuildViewModel(viewModel));

            var script = await _context.Scripts.FindAsync(viewModel.Id);

            if (script == null)
                return NotFound();

            script.Name = viewModel.Name;
            script.Code = viewModel.Code;
            script.WorkspaceId = viewModel.WorkspaceId;
            script.TestSuiteId = viewModel.TestSuiteId;
            script.CategoryId = viewModel.CategoryId;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Script '{script.Name}' bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var script = await _context.Scripts.FindAsync(id);

            if (script != null)
            {
                _context.Scripts.Remove(script);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Script '{script.Name}' verwijderd.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Run(int id, string environment = "Dev")
        {
            _backgroundJobs.Enqueue<RunService>(service => service.ExecuteRunByScriptId(id, environment));

            TempData["Message"] = $"Script {id} is ingepland voor {environment}.";
            return RedirectToAction(nameof(Index));
        }

        private void AddScriptValidationErrors(string? scriptJson)
        {
            foreach (var error in _scriptParser.Validate(scriptJson))
            {
                ModelState.AddModelError(nameof(ScriptFormViewModel.Code), error);
            }
        }

        private async Task<ScriptFormViewModel> BuildViewModel(ScriptFormViewModel? vm = null)
        {
            vm ??= new ScriptFormViewModel();

            vm.Workspaces = await _context.Workspaces
                .AsNoTracking()
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            vm.TestSuites = await _context.TestSuites
                .AsNoTracking()
                .Select(ts => new SelectListItem { Value = ts.Id.ToString(), Text = ts.Name })
                .ToListAsync();

            vm.Categories = await _context.Categories
                .AsNoTracking()
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            return vm;
        }
    }
}
