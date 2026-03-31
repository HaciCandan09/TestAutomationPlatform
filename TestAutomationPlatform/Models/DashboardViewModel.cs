namespace TestAutomationPlatform.Models
{
    public class DashboardViewModel
    {
        public int Id { get; set; }
        public int ScriptId { get; set; }

        public string ScriptName { get; set; }
        public string WorkspaceName { get; set; }
        public string TestSuiteName { get; set; }
        public string CategoryName { get; set; }

        public string Environment { get; set; }
        public string Status { get; set; }
        public string Log { get; set; }
        public double ExecutionTime { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}