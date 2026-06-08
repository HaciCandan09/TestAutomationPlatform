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
            var result = await ExecuteSingleScript(script, environment);

            if (result.status == "Fail")
            {
                hasFailures = true;
            }
        }

        run.Status = hasFailures ? "Completed with failures" : "Completed";
        await _context.SaveChangesAsync();
    }

    public async Task ExecuteRunByScriptId(int scriptId, string environment = "Dev")
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

        run.Status = result.status == "Fail" ? "Completed with failures" : "Completed";
        await _context.SaveChangesAsync();
    }

    private async Task<(string status, string log, double time,string screenshotPath)> ExecuteSingleScript(Script script, string environment)
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

        return result;
    }
}