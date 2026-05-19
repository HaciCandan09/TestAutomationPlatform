using Microsoft.Playwright;
using System.Diagnostics;
using System.Text;
using TestAutomationPlatform.DTO;
using TestAutomationPlatform.Services;

public class TestExecutionService
{
    private const int DefaultTimeoutMs = 5000;
    private readonly ScriptParser _parser;
    private readonly ILogger<TestExecutionService> _logger;

    public TestExecutionService(ScriptParser parser, ILogger<TestExecutionService> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    public async Task<(string status, string log, double time, string screenshotPath)> RunScript(string scriptJson)
    {
        _logger.LogInformation("RunScript started.");

        var stopwatch = Stopwatch.StartNew();
        var log = new StringBuilder();

        var basePath = Directory.GetCurrentDirectory();
        var screenshotsPath = Path.Combine(basePath, "Screenshots");
        Directory.CreateDirectory(screenshotsPath);

        var folderName = $"run_{DateTime.UtcNow.Ticks}";
        var runFolder = Path.Combine(screenshotsPath, folderName);
        Directory.CreateDirectory(runFolder);

        IPage? page = null;
        IBrowser? browser = null;

        try
        {
            var validationErrors = _parser.Validate(scriptJson);
            if (validationErrors.Count > 0)
            {
                throw new Exception(string.Join(" ", validationErrors));
            }

            var steps = _parser.Parse(scriptJson);
            _logger.LogInformation("Parsed {StepCount} test steps.", steps.Count);

            using var playwright = await Playwright.CreateAsync();

            browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });

            page = await browser.NewPageAsync();

            for (var stepIndex = 0; stepIndex < steps.Count; stepIndex++)
            {
                var stepNumber = stepIndex + 1;
                var step = steps[stepIndex];
                await ExecuteStep(page, step, stepNumber, runFolder, log);
            }

            stopwatch.Stop();
            await browser.CloseAsync();

            return ("Pass", log.ToString(), stopwatch.Elapsed.TotalSeconds, folderName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            log.AppendLine($"[{DateTime.UtcNow}] ERROR: {ex.Message}");
            _logger.LogError(ex, "Script execution failed.");

            if (page != null)
                await SafeScreenshot(page, runFolder, "error.png");

            if (browser != null)
                await browser.CloseAsync();

            return ("Fail", log.ToString(), stopwatch.Elapsed.TotalSeconds, folderName);
        }
    }

    private async Task ExecuteStep(IPage page, TestStep step, int stepNumber, string runFolder, StringBuilder log)
    {
        var action = step.Action?.ToLowerInvariant() ?? string.Empty;
        var timeout = step.Timeout ?? DefaultTimeoutMs;

        _logger.LogInformation("Executing step {StepNumber}: {Action}.", stepNumber, action);

        switch (action)
        {
            case "goto":
                await page.GotoAsync(step.Value!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Navigated to {step.Value}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_goto.png");
                break;

            case "click":
                await page.ClickAsync(step.Selector!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Clicked {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_click.png");
                break;

            case "fill":
                await page.FillAsync(step.Selector!, step.Value!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Filled {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_fill.png");
                break;

            case "press":
                await page.PressAsync(step.Selector!, step.Value!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Pressed {step.Value} on {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_press.png");
                break;

            case "wait":
                var waitMs = int.Parse(step.Value!);
                await Task.Delay(waitMs);
                log.AppendLine($"[{DateTime.UtcNow}] Waited {waitMs} ms");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_wait.png");
                break;

            case "waitforselector":
                await page.Locator(step.Selector!).WaitForAsync(new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Selector appeared: {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_waitforselector.png");
                break;

            case "asserttitle":
                var title = await page.TitleAsync();
                if (!title.Contains(step.Value!, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Title mismatch. Expected '{step.Value}', actual '{title}'.");

                log.AppendLine($"[{DateTime.UtcNow}] Title assertion passed: {step.Value}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_asserttitle.png");
                break;

            case "asserturl":
                var url = page.Url;
                if (!url.Contains(step.Value!, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"URL mismatch. Expected '{step.Value}', actual '{url}'.");

                log.AppendLine($"[{DateTime.UtcNow}] URL assertion passed: {step.Value}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_asserturl.png");
                break;

            case "assertelement":
                var state = step.Value?.ToLowerInvariant() == "hidden"
                    ? WaitForSelectorState.Hidden
                    : WaitForSelectorState.Visible;

                await page.Locator(step.Selector!).WaitForAsync(new() { State = state, Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Element state assertion passed: {step.Selector} is {state}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_assertelement.png");
                break;

            case "asserttext":
                var text = await page.Locator(step.Selector!).InnerTextAsync(new() { Timeout = timeout });
                if (!text.Contains(step.Value!, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Text mismatch on {step.Selector}. Expected '{step.Value}', actual '{text}'.");

                log.AppendLine($"[{DateTime.UtcNow}] Text assertion passed on {step.Selector}: {step.Value}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_asserttext.png");
                break;

            case "select":
                await page.SelectOptionAsync(step.Selector!, step.Value, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Selected {step.Value} in {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_select.png");
                break;

            case "check":
                await page.CheckAsync(step.Selector!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Checked {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_check.png");
                break;

            case "uncheck":
                await page.UncheckAsync(step.Selector!, new() { Timeout = timeout });
                log.AppendLine($"[{DateTime.UtcNow}] Unchecked {step.Selector}");
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_uncheck.png");
                break;

            case "screenshot":
                await SafeScreenshot(page, runFolder, $"step_{stepNumber}_screenshot.png");
                log.AppendLine($"[{DateTime.UtcNow}] Screenshot captured");
                break;

            default:
                throw new Exception($"Onbekende actie: {step.Action}");
        }
    }

    private async Task SafeScreenshot(IPage page, string folder, string fileName)
    {
        try
        {
            var fullPath = Path.Combine(folder, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath
            });

            _logger.LogInformation("Screenshot saved: {Path}", fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Screenshot failed.");
        }
    }
}
