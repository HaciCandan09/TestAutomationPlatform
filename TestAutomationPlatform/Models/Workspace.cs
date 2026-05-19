using TestAutomationPlatform.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<Script> Scripts { get; set; } = new();
    public List<TestSuite> TestSuites { get; set; } = new();
}