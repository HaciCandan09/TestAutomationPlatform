using System.ComponentModel.DataAnnotations;

namespace TestAutomationPlatform.Models
{
    public class Defect
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DefectStatus Status { get; set; } = DefectStatus.Open;

        public DefectPriority Priority { get; set; } = DefectPriority.Middel;

        public int RunResultId { get; set; }

        public RunResult RunResult { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
