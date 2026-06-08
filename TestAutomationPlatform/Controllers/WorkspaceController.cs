using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly AppDbContext _context;

        public WorkspaceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var workspaces = await _context.Workspaces
                .OrderBy(w => w.Name)
                .ToListAsync();

            return View(workspaces);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Workspacenaam is verplicht.";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.Workspaces
                .AnyAsync(w => w.Name.ToLower() == name.ToLower());

            if (exists)
            {
                TempData["ErrorMessage"] = "Er bestaat al een workspace met deze naam.";
                return RedirectToAction(nameof(Index));
            }

            var workspace = new Workspace
            {
                Name = name
            };

            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Workspace is succesvol aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Scripts)
                .Include(w => w.Categories)
                .Include(w => w.TestSuites)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workspace == null)
            {
                TempData["ErrorMessage"] = "Workspace niet gevonden.";
                return RedirectToAction(nameof(Index));
            }

            if (workspace.Scripts.Any() || workspace.Categories.Any() || workspace.TestSuites.Any())
            {
                TempData["ErrorMessage"] = "Deze workspace kan niet worden verwijderd, omdat er nog scripts, categorieën of test-suites aan gekoppeld zijn.";
                return RedirectToAction(nameof(Index));
            }

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Workspace is succesvol verwijderd.";
            return RedirectToAction(nameof(Index));
        }
    }
}