namespace TestAutomationPlatform.Models
{
    public class TestSuite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public List<Script> Scripts { get; set; } = new();
    }
}