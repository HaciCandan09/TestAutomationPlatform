namespace TestAutomationPlatform.Models
{
    public class Run
    {
        public int Id { get; set; }
        public string Environment { get; set; } = "Dev";
        public DateTime ScheduledAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
    }
}