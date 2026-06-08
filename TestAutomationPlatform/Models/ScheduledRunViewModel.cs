using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestAutomationPlatform.Models;

public class ScheduledRunViewModel
{
    public List<ScheduledRun> ScheduledRuns { get; set; } = new();

    public List<SelectListItem> Scripts { get; set; } = new();

    public int ScriptId { get; set; }

    public string Environment { get; set; } = "Dev";

    public string Interval { get; set; } = "Every5Minutes";
}