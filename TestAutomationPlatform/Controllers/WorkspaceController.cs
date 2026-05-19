using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;

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
                .AsNoTracking()
                .Include(w => w.TestSuites)
                .Include(w => w.Scripts)
                .OrderBy(w => w.Name)
                .ToListAsync();

            return View(workspaces);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Workspace());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workspace workspace)
        {
            if (string.IsNullOrWhiteSpace(workspace.Name))
            {
                ModelState.AddModelError(nameof(workspace.Name), "Workspace naam is verplicht.");
            }

            if (!ModelState.IsValid)
                return View(workspace);

            workspace.Name = workspace.Name.Trim();
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Workspace '{workspace.Name}' aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var workspace = await _context.Workspaces.FindAsync(id);
            return workspace == null ? NotFound() : View(workspace);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Workspace workspace)
        {
            if (string.IsNullOrWhiteSpace(workspace.Name))
            {
                ModelState.AddModelError(nameof(workspace.Name), "Workspace naam is verplicht.");
            }

            if (!ModelState.IsValid)
                return View(workspace);

            var existing = await _context.Workspaces.FindAsync(workspace.Id);
            if (existing == null)
                return NotFound();

            existing.Name = workspace.Name.Trim();
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Workspace '{existing.Name}' bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Scripts)
                .Include(w => w.TestSuites)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (workspace == null)
                return RedirectToAction(nameof(Index));

            if (workspace.Scripts.Any() || workspace.TestSuites.Any())
            {
                TempData["Error"] = "Workspace kan niet verwijderd worden zolang er scripts of suites aan gekoppeld zijn.";
                return RedirectToAction(nameof(Index));
            }

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Workspace '{workspace.Name}' verwijderd.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/api/workspace")]
        public async Task<IActionResult> GetAll()
        {
            var workspaces = await _context.Workspaces.AsNoTracking().OrderBy(w => w.Name).ToListAsync();
            return Ok(workspaces);
        }

        [HttpPost("/api/workspace")]
        public async Task<IActionResult> CreateApi([FromBody] Workspace workspace)
        {
            workspace.Name = workspace.Name.Trim();
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return Ok(workspace);
        }
    }
}