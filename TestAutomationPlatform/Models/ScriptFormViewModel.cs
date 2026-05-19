using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestAutomationPlatform.Models
{
    public class ScriptFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Script naam is verplicht.")]
        [StringLength(120, ErrorMessage = "Script naam mag maximaal 120 tekens zijn.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Script JSON is verplicht.")]
        public string Code { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Kies een workspace.")]
        public int WorkspaceId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kies een testsuite.")]
        public int TestSuiteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Kies een categorie.")]
        public int CategoryId { get; set; }

        public List<SelectListItem> Workspaces { get; set; } = new();
        public List<SelectListItem> TestSuites { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }
}
