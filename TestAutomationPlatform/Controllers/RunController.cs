using Microsoft.AspNetCore.Mvc;

namespace TestAutomationPlatform.Controllers
{
    public class RunController : Controller
    {
        private readonly RunService _runService;

        public RunController(RunService runService)
        {
            _runService = runService;
        }

        [HttpPost]
        public async Task<IActionResult> StartRun(string environment)
        {
            await _runService.ExecuteRun(environment);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}