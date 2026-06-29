using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers;

public class TestStructureController : Controller
{
    private readonly AppDbContext _context;

    public TestStructureController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? workspaceId)
    {
        if (workspaceId.HasValue)
        {
            var exists = await _context.Workspaces
                .AnyAsync(w => w.Id == workspaceId.Value);

            if (!exists)
                return NotFound();
        }

        return View(await BuildViewModel(workspaceId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(
    TestStructureViewModel viewModel)
    {
        var workspaceId = viewModel.SelectedWorkspaceId
            ?? viewModel.CategoryWorkspaceId;

        if (string.IsNullOrWhiteSpace(viewModel.NewCategoryName))
        {
            TempData["ErrorMessage"] = "Categorienaam is verplicht.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        var workspaceExists = await _context.Workspaces
            .AnyAsync(w => w.Id == viewModel.CategoryWorkspaceId);

        if (!workspaceExists)
        {
            TempData["ErrorMessage"] = "Ongeldige workspace.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        var category = new Category
        {
            Name = viewModel.NewCategoryName,
            WorkspaceId = viewModel.CategoryWorkspaceId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Categorie is succesvol aangemaakt.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTestSuite(
    TestStructureViewModel viewModel)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(
                c => c.Id == viewModel.TestSuiteCategoryId);

        var workspaceId = viewModel.SelectedWorkspaceId
            ?? category?.WorkspaceId;

        if (string.IsNullOrWhiteSpace(viewModel.NewTestSuiteName))
        {
            TempData["ErrorMessage"] =
                "Test-suite naam is verplicht.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        if (category == null)
        {
            TempData["ErrorMessage"] =
                "De geselecteerde categorie bestaat niet.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        if (viewModel.SelectedWorkspaceId.HasValue &&
            category.WorkspaceId != viewModel.SelectedWorkspaceId.Value)
        {
            TempData["ErrorMessage"] =
                "Deze categorie hoort niet bij de geopende workspace.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        var testSuite = new TestSuite
        {
            Name = viewModel.NewTestSuiteName,
            CategoryId = category.Id,

            // Tijdelijk totdat de oude directe relatie wordt verwijderd.
            WorkspaceId = category.WorkspaceId
        };

        _context.TestSuites.Add(testSuite);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Test-suite is succesvol aangemaakt.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(
    int id,
    int? workspaceId)
    {
        var category = await _context.Categories
            .Include(c => c.TestSuites)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            TempData["ErrorMessage"] = "Categorie niet gevonden.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        var hasScripts = await _context.Scripts
            .AnyAsync(s => s.CategoryId == id);

        if (hasScripts)
        {
            TempData["ErrorMessage"] =
                "Deze categorie kan niet worden verwijderd omdat er nog scripts aan gekoppeld zijn.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        if (category.TestSuites.Any())
        {
            TempData["ErrorMessage"] =
                "Deze categorie kan niet worden verwijderd omdat er nog test-suites in staan.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Categorie is verwijderd.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestSuite(
    int id,
    int? workspaceId)
    {
        var testSuite = await _context.TestSuites
            .Include(ts => ts.Scripts)
            .FirstOrDefaultAsync(ts => ts.Id == id);

        if (testSuite == null)
        {
            TempData["ErrorMessage"] = "Test-suite niet gevonden.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        if (testSuite.Scripts.Any())
        {
            TempData["ErrorMessage"] =
                "Deze test-suite kan niet worden verwijderd omdat er nog scripts in staan.";

            return RedirectToAction(
                nameof(Index),
                new { workspaceId });
        }

        _context.TestSuites.Remove(testSuite);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Test-suite is verwijderd.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }

    private async Task<TestStructureViewModel> BuildViewModel(
     int? workspaceId)
    {
        var workspaces = await _context.Workspaces
            .OrderBy(w => w.Name)
            .Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name
            })
            .ToListAsync();

        var categoriesQuery = _context.Categories
            .Include(c => c.Workspace)
            .AsQueryable();

        var testSuitesQuery = _context.TestSuites
            .Include(ts => ts.Workspace)
            .Include(ts => ts.Category)
                .ThenInclude(c => c.Workspace)
            .AsQueryable();

        if (workspaceId.HasValue)
        {
            categoriesQuery = categoriesQuery
                .Where(c => c.WorkspaceId == workspaceId.Value);

            testSuitesQuery = testSuitesQuery
                .Where(ts => ts.WorkspaceId == workspaceId.Value);
        }

        var categories = await categoriesQuery
            .OrderBy(c => c.Name)
            .ToListAsync();

        var testSuites = await testSuitesQuery
            .OrderBy(ts => ts.Name)
            .ToListAsync();

        return new TestStructureViewModel
        {
            Categories = categories,
            TestSuites = testSuites,
            Workspaces = workspaces,

            SelectedWorkspaceId = workspaceId,

            SelectedWorkspaceName = workspaceId.HasValue
                ? workspaces
                    .FirstOrDefault(w =>
                        w.Value == workspaceId.Value.ToString())?.Text
                : null,

            CategoryWorkspaceId = workspaceId ?? 0,

            CategoryOptions = categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Workspace.Name + " / " + c.Name
                })
                .ToList()
        };
    }
}