using System.Text.Json.Serialization;
using TestAutomationPlatform.Models;

public class TestSuite
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int WorkspaceId { get; set; }

    [JsonIgnore]
    public Workspace? Workspace { get; set; }

    public List<Script> Scripts { get; set; } = new();
}