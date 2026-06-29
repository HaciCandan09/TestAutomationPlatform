using Microsoft.AspNetCore.SignalR;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Hubs;
using TestAutomationPlatform.Models;
using TestAutomationPlatform.Repository;

namespace TestAutomationPlatform.Services
{
    public class RunService
    {
        private readonly IScriptRepository _scriptRepo;
        private readonly TestExecutionService _execution;
        private readonly AppDbContext _context;
        private readonly IHubContext<TestResultHub> _hubContext;

        public RunService(
            IScriptRepository scriptRepo,
            TestExecutionService execution,
            AppDbContext context,
            IHubContext<TestResultHub> hubContext)
        {
            _scriptRepo = scriptRepo;
            _execution = execution;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task ExecuteRun(string environment = "Dev")
        {
            var allowedEnvironments = new[] { "Dev", "Preprod", "Prod" };

            if (!allowedEnvironments.Contains(environment))
            {
                environment = "Dev";
            }

            var run = new Run
            {
                Environment = environment,
                ScheduledAt = DateTime.Now,
                Status = "Running"
            };

            _context.Runs.Add(run);
            await _context.SaveChangesAsync();

            var scripts = await _scriptRepo.GetAll();
            bool hasFailures = false;

            foreach (var script in scripts)
            {
                var result = await ExecuteSingleScript(script, environment);

                if (result.status == "Fail")
                {
                    hasFailures = true;
                }
            }

            run.Status = hasFailures
                ? "Completed with failures"
                : "Completed";

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TestResultUpdated");
        }

        public async Task ExecuteRunByScriptId(
            int scriptId,
            string environment = "Dev")
        {
            var allowedEnvironments = new[] { "Dev", "Preprod", "Prod" };

            if (!allowedEnvironments.Contains(environment))
            {
                environment = "Dev";
            }

            var script = await _scriptRepo.GetById(scriptId);

            if (script == null)
            {
                throw new Exception($"Script met ID {scriptId} niet gevonden.");
            }

            var run = new Run
            {
                Environment = environment,
                ScheduledAt = DateTime.Now,
                Status = "Running"
            };

            _context.Runs.Add(run);
            await _context.SaveChangesAsync();

            var result = await ExecuteSingleScript(script, environment);

            run.Status = result.status == "Fail"
                ? "Completed with failures"
                : "Completed";

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TestResultUpdated");
        }

        public async Task ExecuteScheduledScript(
            int scriptId,
            string environment)
        {
            await ExecuteRunByScriptId(scriptId, environment);
        }

        private async Task<(string status, string log, double time, string screenshotPath)>
            ExecuteSingleScript(Script script, string environment)
        {
            var result = await _execution.RunScript(script.Code);

            var runResult = new RunResult
            {
                ScriptId = script.Id,
                Environment = environment,
                Status = result.status,
                Log = result.log,
                ExecutionTime = result.time,
                ExecutedAt = DateTime.Now,
                ScreenshotPath = result.screenshotPath
            };

            _context.RunResults.Add(runResult);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("TestResultUpdated");

            return result;
        }
    }
}