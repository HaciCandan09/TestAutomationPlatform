namespace TestAutomationPlatform.Models;

public class RunResult
{
    public int Id { get; set; }
    public int ScriptId { get; set; }
    public string Environment { get; set; } = "Dev";
    public string Status { get; set; } = string.Empty;
    public string Log { get; set; } = string.Empty;
    public string? ScreenshotPath { get; set; }
    public double ExecutionTime { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
