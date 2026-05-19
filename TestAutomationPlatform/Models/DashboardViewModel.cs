namespace TestAutomationPlatform.Models
{
    public class DashboardViewModel
    {
        public int Id { get; set; }
        public int ScriptId { get; set; }

        public string ScriptName { get; set; } = string.Empty;
        public string WorkspaceName { get; set; } = string.Empty;
        public string TestSuiteName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public string Environment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Log { get; set; } = string.Empty;

        public string? ScreenshotPath { get; set; }
        public double ExecutionTime { get; set; }
        public DateTime ExecutedAt { get; set; }

        public bool Passed => string.Equals(Status, "Pass", StringComparison.OrdinalIgnoreCase);
        public bool Failed => string.Equals(Status, "Fail", StringComparison.OrdinalIgnoreCase);
    }
}
