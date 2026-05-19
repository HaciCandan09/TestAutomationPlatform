using Microsoft.Playwright;
using System.Diagnostics;
using System.Text;
using TestAutomationPlatform.Services;

public class TestExecutionService
{
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
            var steps = _parser.Parse(scriptJson);
            _logger.LogInformation("Parsed {StepCount} test steps.", steps.Count);

            using var playwright = await Playwright.CreateAsync();

            browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });

            page = await browser.NewPageAsync();

            int stepIndex = 0;

            foreach (var step in steps)
            {
                stepIndex++;

                if (step == null || string.IsNullOrWhiteSpace(step.Action))
                {
                    throw new Exception($"Stap {stepIndex} is ongeldig: Action ontbreekt.");
                }

                _logger.LogInformation("Executing step action {Action}.", step.Action);

                switch (step.Action.ToLower())
                {
                    case "goto":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor goto.");

                        await page.GotoAsync(step.Value);
                        log.AppendLine($"[{DateTime.UtcNow}] Navigated to {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_goto.png");
                        break;

                    case "click":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor click.");

                        await page.ClickAsync(step.Selector);
                        log.AppendLine($"[{DateTime.UtcNow}] Clicked {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_click.png");
                        break;

                    case "fill":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor fill.");

                        if (step.Value == null)
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor fill.");

                        await page.FillAsync(step.Selector, step.Value);
                        log.AppendLine($"[{DateTime.UtcNow}] Filled {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_fill.png");
                        break;

                    case "press":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor press.");

                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor press.");

                        await page.PressAsync(step.Selector, step.Value);
                        log.AppendLine($"[{DateTime.UtcNow}] Pressed {step.Value} on {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_press.png");
                        break;

                    case "asserttitle":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor asserttitle.");

                        var title = await page.TitleAsync();

                        if (!title.Contains(step.Value))
                            throw new Exception($"Title mismatch: {title}");

                        log.AppendLine($"[{DateTime.UtcNow}] Title assertion passed: {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_asserttitle.png");
                        break;

                    case "wait":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor wait.");

                        if (!int.TryParse(step.Value, out int waitMs))
                            throw new Exception($"Stap {stepIndex}: Wait value ongeldig.");

                        await Task.Delay(waitMs);
                        log.AppendLine($"[{DateTime.UtcNow}] Waited {waitMs} ms");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_wait.png");
                        break;

                    case "assertelement":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt.");

                        var locator = page.Locator(step.Selector);

                        var state = step.Value?.ToLower() ?? "visible";

                        if (state == "hidden")
                        {
                            await locator.WaitForAsync(new()
                            {
                                State = WaitForSelectorState.Hidden,
                                Timeout = 5000
                            });

                            log.AppendLine($"[{DateTime.UtcNow}] Element hidden: {step.Selector}");
                        }
                        else
                        {
                            await locator.WaitForAsync(new()
                            {
                                State = WaitForSelectorState.Visible,
                                Timeout = 5000
                            });

                            log.AppendLine($"[{DateTime.UtcNow}] Element visible: {step.Selector}");
                        }

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_assertelement.png");
                        break;

                    default:
                        throw new Exception($"Onbekende actie: {step.Action}");
                }
            }

            stopwatch.Stop();

            if (browser != null)
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
