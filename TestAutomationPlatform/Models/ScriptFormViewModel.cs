using Microsoft.AspNetCore.Mvc.Rendering;

namespace TestAutomationPlatform.Models
{
    public class ScriptFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public int WorkspaceId { get; set; }
        public int TestSuiteId { get; set; }
        public int CategoryId { get; set; }

        public List<SelectListItem> Workspaces { get; set; } = new();
        public List<SelectListItem> TestSuites { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
    }
}