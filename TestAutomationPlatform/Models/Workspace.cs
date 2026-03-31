namespace TestAutomationPlatform.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public List<TestSuite> TestSuites { get; set; } = new();
        public List<Script> Scripts { get; set; } = new();
    }
}