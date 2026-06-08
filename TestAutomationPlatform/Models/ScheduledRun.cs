namespace TestAutomationPlatform.Models;

public class ScheduledRun
{
    public int Id { get; set; }

    public int ScriptId { get; set; }
    public Script Script { get; set; } = null!;

    public string Environment { get; set; } = "Dev";

    public string CronExpression { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}