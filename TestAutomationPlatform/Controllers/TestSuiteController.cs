using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;

namespace TestAutomationPlatform.Controllers
{
    public class TestSuiteController : Controller
    {
        private readonly AppDbContext _context;

        public TestSuiteController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var suites = await _context.TestSuites
                .AsNoTracking()
                .Include(ts => ts.Workspace)
                .Include(ts => ts.Scripts)
                .OrderBy(ts => ts.Workspace!.Name)
                .ThenBy(ts => ts.Name)
                .ToListAsync();

            return View(suites);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateWorkspaces();
            return View(new TestSuite());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestSuite suite)
        {
            ValidateSuite(suite);

            if (!ModelState.IsValid)
            {
                await PopulateWorkspaces(suite.WorkspaceId);
                return View(suite);
            }

            suite.Name = suite.Name.Trim();
            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"TestSuite '{suite.Name}' aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var suite = await _context.TestSuites.FindAsync(id);
            if (suite == null)
                return NotFound();

            await PopulateWorkspaces(suite.WorkspaceId);
            return View(suite);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestSuite suite)
        {
            ValidateSuite(suite);

            if (!ModelState.IsValid)
            {
                await PopulateWorkspaces(suite.WorkspaceId);
                return View(suite);
            }

            var existing = await _context.TestSuites.FindAsync(suite.Id);
            if (existing == null)
                return NotFound();

            existing.Name = suite.Name.Trim();
            existing.WorkspaceId = suite.WorkspaceId;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"TestSuite '{existing.Name}' bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var suite = await _context.TestSuites
                .Include(ts => ts.Scripts)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (suite == null)
                return RedirectToAction(nameof(Index));

            if (suite.Scripts.Any())
            {
                TempData["Error"] = "TestSuite kan niet verwijderd worden zolang er scripts aan gekoppeld zijn.";
                return RedirectToAction(nameof(Index));
            }

            _context.TestSuites.Remove(suite);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"TestSuite '{suite.Name}' verwijderd.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/api/testsuite")]
        public async Task<IActionResult> GetAll()
        {
            var suites = await _context.TestSuites
                .AsNoTracking()
                .Include(ts => ts.Workspace)
                .OrderBy(ts => ts.Name)
                .ToListAsync();

            return Ok(suites);
        }

        [HttpPost("/api/testsuite")]
        public async Task<IActionResult> CreateApi([FromBody] TestSuite suite)
        {
            suite.Name = suite.Name.Trim();
            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();
            return Ok(suite);
        }

        private async Task PopulateWorkspaces(int? selectedWorkspaceId = null)
        {
            ViewBag.Workspaces = await _context.Workspaces
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name,
                    Selected = selectedWorkspaceId == w.Id
                })
                .ToListAsync();
        }

        private void ValidateSuite(TestSuite suite)
        {
            if (string.IsNullOrWhiteSpace(suite.Name))
            {
                ModelState.AddModelError(nameof(suite.Name), "TestSuite naam is verplicht.");
            }

            if (suite.WorkspaceId <= 0)
            {
                ModelState.AddModelError(nameof(suite.WorkspaceId), "Kies een workspace.");
            }
        }
    }
}
