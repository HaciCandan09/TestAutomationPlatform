using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestAutomationPlatform.Models;

public class TestStructureViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<TestSuite> TestSuites { get; set; } = new();

    public List<SelectListItem> Workspaces { get; set; } = new();

    public string NewCategoryName { get; set; } = string.Empty;
    public int CategoryWorkspaceId { get; set; }

    public string NewTestSuiteName { get; set; } = string.Empty;
    public int TestSuiteWorkspaceId { get; set; }
}