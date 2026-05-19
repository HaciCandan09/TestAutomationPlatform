using Hangfire;
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

        private readonly IBackgroundJobClient _backgroundJobs;

        public RunController(IBackgroundJobClient backgroundJobs)
        {
            _backgroundJobs = backgroundJobs;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartRun(string? environment)
        {
            var normalizedEnvironment = NormalizeEnvironment(environment);

            _backgroundJobs.Enqueue<RunService>(service => service.ExecuteRun(normalizedEnvironment));
            TempData["Message"] = $"Test run voor {normalizedEnvironment} is ingepland.";

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
