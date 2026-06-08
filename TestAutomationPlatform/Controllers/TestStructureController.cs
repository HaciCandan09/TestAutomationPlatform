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

    public async Task<IActionResult> Index()
    {
        var viewModel = await BuildViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(TestStructureViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.NewCategoryName))
        {
            TempData["ErrorMessage"] = "Categorienaam is verplicht.";
            return RedirectToAction(nameof(Index));
        }

        var workspaceExists = await _context.Workspaces
            .AnyAsync(w => w.Id == viewModel.CategoryWorkspaceId);

        if (!workspaceExists)
        {
            TempData["ErrorMessage"] = "Selecteer een geldige workspace voor de categorie.";
            return RedirectToAction(nameof(Index));
        }

        var category = new Category
        {
            Name = viewModel.NewCategoryName,
            WorkspaceId = viewModel.CategoryWorkspaceId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Categorie is succesvol aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTestSuite(TestStructureViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.NewTestSuiteName))
        {
            TempData["ErrorMessage"] = "Test-suite naam is verplicht.";
            return RedirectToAction(nameof(Index));
        }

        var workspaceExists = await _context.Workspaces
            .AnyAsync(w => w.Id == viewModel.TestSuiteWorkspaceId);

        if (!workspaceExists)
        {
            TempData["ErrorMessage"] = "Selecteer een geldige workspace voor de test-suite.";
            return RedirectToAction(nameof(Index));
        }

        var testSuite = new TestSuite
        {
            Name = viewModel.NewTestSuiteName,
            WorkspaceId = viewModel.TestSuiteWorkspaceId
        };

        _context.TestSuites.Add(testSuite);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Test-suite is succesvol aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Categorie is verwijderd.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestSuite(int id)
    {
        var testSuite = await _context.TestSuites.FindAsync(id);

        if (testSuite != null)
        {
            _context.TestSuites.Remove(testSuite);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Test-suite is verwijderd.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<TestStructureViewModel> BuildViewModel()
    {
        var workspaces = await _context.Workspaces
            .OrderBy(w => w.Name)
            .Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = w.Name
            })
            .ToListAsync();

        return new TestStructureViewModel
        {
            Categories = await _context.Categories
                .Include(c => c.Workspace)
                .OrderBy(c => c.Workspace.Name)
                .ThenBy(c => c.Name)
                .ToListAsync(),

            TestSuites = await _context.TestSuites
                .Include(ts => ts.Workspace)
                .OrderBy(ts => ts.Workspace.Name)
                .ThenBy(ts => ts.Name)
                .ToListAsync(),

            Workspaces = workspaces
        };
    }
}