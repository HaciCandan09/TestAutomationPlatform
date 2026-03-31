using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    public class ScriptsFormController : Controller
    {
        private readonly AppDbContext _context;

        public ScriptsFormController(AppDbContext context)
        {
            _context = context;
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
            var viewModel = new ScriptFormViewModel
            {
                Workspaces = await _context.Workspaces
                    .Select(w => new SelectListItem
                    {
                        Value = w.Id.ToString(),
                        Text = w.Name
                    })
                    .ToListAsync(),

                TestSuites = await _context.TestSuites
                    .Select(ts => new SelectListItem
                    {
                        Value = ts.Id.ToString(),
                        Text = ts.Name
                    })
                    .ToListAsync(),

                Categories = await _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScriptFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Workspaces = await _context.Workspaces
                    .Select(w => new SelectListItem
                    {
                        Value = w.Id.ToString(),
                        Text = w.Name
                    })
                    .ToListAsync();

                viewModel.TestSuites = await _context.TestSuites
                    .Select(ts => new SelectListItem
                    {
                        Value = ts.Id.ToString(),
                        Text = ts.Name
                    })
                    .ToListAsync();

                viewModel.Categories = await _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(viewModel);
            }

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
    }
}