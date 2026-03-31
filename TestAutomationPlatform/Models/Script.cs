namespace TestAutomationPlatform.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        public int TestSuiteId { get; set; }
        public TestSuite? TestSuite { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}