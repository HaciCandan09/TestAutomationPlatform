using TestAutomationPlatform.Repository;
using TestAutomationPlatform.Services;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

public class RunService
{
    private readonly IScriptRepository _scriptRepo;
    private readonly TestExecutionService _execution;
    private readonly AppDbContext _context;

    public RunService(IScriptRepository scriptRepo,
                      TestExecutionService execution,
                      AppDbContext context)
    {
        _scriptRepo = scriptRepo;
        _execution = execution;
        _context = context;
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
            var result = await _execution.RunScript(script.Code);

            if (result.status == "Fail")
            {
                hasFailures = true;
            }

            var runResult = new RunResult
            {
                ScriptId = script.Id,
                Environment = environment,
                Status = result.status,
                Log = result.log,
                ExecutionTime = result.time,
                ExecutedAt = DateTime.Now
            };

            _context.RunResults.Add(runResult);
        }

        run.Status = hasFailures ? "Completed with failures" : "Completed";
        await _context.SaveChangesAsync();
    }
}