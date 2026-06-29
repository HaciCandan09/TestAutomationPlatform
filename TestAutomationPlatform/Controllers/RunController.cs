using Microsoft.AspNetCore.Mvc;
using TestAutomationPlatform.Services;
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