using Microsoft.AspNetCore.Mvc;

namespace TestAutomationPlatform.Controllers
{
    public class RunController : Controller
    {
        private static readonly HashSet<string> AllowedEnvironments = new(StringComparer.OrdinalIgnoreCase)
        {
            "Dev",
            "Preprod",
            "Prod"
        };

        private readonly RunService _runService;

        public RunController(RunService runService)
        {
            _runService = runService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartRun(string? environment)
        {
            var normalizedEnvironment = NormalizeEnvironment(environment);

            await _runService.ExecuteRun(normalizedEnvironment);
            TempData["Message"] = $"Test run gestart voor {normalizedEnvironment}.";

            return RedirectToAction("Index", "Dashboard", new { environment = normalizedEnvironment });
        }

        private static string NormalizeEnvironment(string? environment)
        {
            if (string.IsNullOrWhiteSpace(environment))
            {
                return "Dev";
            }

            return AllowedEnvironments.TryGetValue(environment, out var normalized)
                ? normalized
                : "Dev";
        }
    }
}
