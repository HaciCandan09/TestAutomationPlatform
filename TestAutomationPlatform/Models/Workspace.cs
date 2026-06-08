namespace TestAutomationPlatform.Models;

public class Workspace
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Script> Scripts { get; set; } = new List<Script>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<TestSuite> TestSuites { get; set; } = new List<TestSuite>();
}