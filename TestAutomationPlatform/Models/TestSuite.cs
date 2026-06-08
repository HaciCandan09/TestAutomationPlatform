namespace TestAutomationPlatform.Models;

public class TestSuite
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public ICollection<Script> Scripts { get; set; } = new List<Script>();
}