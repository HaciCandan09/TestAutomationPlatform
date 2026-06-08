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

        // CREATE
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(await BuildViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScriptFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(await BuildViewModel(viewModel));

            var script = new Script
            {
                Name = viewModel.Name,
                Code = viewModel.Code,
                WorkspaceId = viewModel.WorkspaceId,
                TestSuiteId = viewModel.TestSuiteId,
                CategoryId = viewModel.CategoryId,
                CreatedAt = DateTime.Now
            };

            _context.Scripts.Add(script);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // EDIT
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

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var script = await _context.Scripts.FindAsync(id);

            if (script != null)
            {
                _context.Scripts.Remove(script);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // RUN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id)
        {
            await _runService.ExecuteRunByScriptId(id);

            TempData["Message"] = $"Script {id} uitgevoerd!";
            return RedirectToAction(nameof(Index));
        }

        
        private async Task<ScriptFormViewModel> BuildViewModel(ScriptFormViewModel vm = null)
        {
            vm ??= new ScriptFormViewModel();

            vm.Workspaces = await _context.Workspaces
                .Select(w => new SelectListItem { Value = w.Id.ToString(), Text = w.Name })
                .ToListAsync();

            vm.TestSuites = await _context.TestSuites
                .Select(ts => new SelectListItem { Value = ts.Id.ToString(), Text = ts.Name })
                .ToListAsync();

            vm.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();

            return vm;
        }
    }
}