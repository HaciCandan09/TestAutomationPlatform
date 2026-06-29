using Microsoft.Playwright;
using System.Diagnostics;
using System.Text;
using TestAutomationPlatform.Services;

public class TestExecutionService
{
    private readonly ScriptParser _parser;

    public TestExecutionService(ScriptParser parser)
    {
        _parser = parser;
    }

    public async Task<(string status, string log, double time, string screenshotPath)> RunScript(string scriptJson)
    {
        Console.WriteLine("RunScript gestart!");

        var stopwatch = Stopwatch.StartNew();
        var log = new StringBuilder();

        var basePath = Directory.GetCurrentDirectory();
        var screenshotsPath = Path.Combine(basePath, "Screenshots");
        Directory.CreateDirectory(screenshotsPath);

        var folderName = $"run_{DateTime.Now.Ticks}";
        var runFolder = Path.Combine(screenshotsPath, folderName);
        Directory.CreateDirectory(runFolder);

        IPage? page = null;
        IBrowser? browser = null;

        try
        {
            var steps = _parser.Parse(scriptJson);
            Console.WriteLine($"Aantal steps: {steps.Count}");

            using var playwright = await Playwright.CreateAsync();

            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-sandbox"
                }
            });

            page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1366,
                    Height = 768
                }
            });

            int stepIndex = 0;

            foreach (var step in steps)
            {
                stepIndex++;

                if (step == null || string.IsNullOrWhiteSpace(step.Action))
                {
                    throw new Exception($"Stap {stepIndex} is ongeldig: Action ontbreekt.");
                }

                Console.WriteLine($"Stap: {step.Action}");

                switch (step.Action.ToLower())
                {
                    case "goto":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor goto.");

                        await page.GotoAsync(step.Value, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.Load,
                            Timeout = 30000
                        });

                        await page.WaitForTimeoutAsync(1000);

                        log.AppendLine($"[{DateTime.Now}] Navigated to {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_goto.png");
                        break;

                    case "click":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor click.");

                        await page.Locator(step.Selector).WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = 10000
                        });

                        IPage? popupPage = null;

                        var popupTask = page.WaitForPopupAsync(new PageWaitForPopupOptions
                        {
                            Timeout = 3000
                        });

                        await page.ClickAsync(step.Selector);

                        try
                        {
                            popupPage = await popupTask;
                        }
                        catch (TimeoutException)
                        {
                            popupPage = null;
                        }

                        if (popupPage != null)
                        {
                            page = popupPage;

                            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                            log.AppendLine($"[{DateTime.Now}] Clicked {step.Selector} and switched to new tab");
                        }
                        else
                        {
                            log.AppendLine($"[{DateTime.Now}] Clicked {step.Selector}");
                        }

                        await page.WaitForTimeoutAsync(1000);

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_click.png");
                        break;

                    case "fill":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor fill.");

                        if (step.Value == null)
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor fill.");

                        await page.Locator(step.Selector).WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = 10000
                        });

                        await page.FillAsync(step.Selector, step.Value);

                        await page.WaitForTimeoutAsync(500);

                        log.AppendLine($"[{DateTime.Now}] Filled {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_fill.png");
                        break;

                    case "press":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor press.");

                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor press.");

                        await page.Locator(step.Selector).WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = 10000
                        });

                        await page.PressAsync(step.Selector, step.Value);

                        await page.WaitForTimeoutAsync(1500);

                        log.AppendLine($"[{DateTime.Now}] Pressed {step.Value} on {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_press.png");
                        break;

                    case "wait":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor wait.");

                        if (!int.TryParse(step.Value, out int waitMs))
                            throw new Exception($"Stap {stepIndex}: Wait value ongeldig.");

                        await page.WaitForTimeoutAsync(waitMs);

                        log.AppendLine($"[{DateTime.Now}] Waited {waitMs} ms");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_wait.png");
                        break;

                    case "waitforselector":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor waitforselector.");

                        await page.Locator(step.Selector).WaitForAsync(new LocatorWaitForOptions
                        {
                            State = WaitForSelectorState.Visible,
                            Timeout = step.Timeout > 0 ? step.Timeout : 10000
                        });

                        log.AppendLine($"[{DateTime.Now}] Waited for selector {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_waitforselector.png");
                        break;

                    case "asserttitle":
                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor asserttitle.");

                        var title = await page.TitleAsync();

                        if (!title.Contains(step.Value, StringComparison.OrdinalIgnoreCase))
                            throw new Exception($"Title mismatch. Verwacht: {step.Value}, gevonden: {title}");

                        log.AppendLine($"[{DateTime.Now}] Title assertion passed: {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_asserttitle.png");
                        break;

                    case "asserttext":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor asserttext.");

                        if (string.IsNullOrWhiteSpace(step.Value))
                            throw new Exception($"Stap {stepIndex}: Value ontbreekt voor asserttext.");

                        var textContent = await page.Locator(step.Selector).TextContentAsync();

                        if (textContent == null || !textContent.Contains(step.Value, StringComparison.OrdinalIgnoreCase))
                            throw new Exception($"Tekst niet gevonden. Verwacht: {step.Value}");

                        log.AppendLine($"[{DateTime.Now}] Text assertion passed: {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_asserttext.png");
                        break;

                    case "assertelement":
                        if (string.IsNullOrWhiteSpace(step.Selector))
                            throw new Exception($"Stap {stepIndex}: Selector ontbreekt voor assertelement.");

                        var locator = page.Locator(step.Selector);
                        var state = step.Value?.ToLower() ?? "visible";

                        if (state == "hidden")
                        {
                            await locator.WaitForAsync(new LocatorWaitForOptions
                            {
                                State = WaitForSelectorState.Hidden,
                                Timeout = 5000
                            });

                            log.AppendLine($"[{DateTime.Now}] Element hidden: {step.Selector}");
                        }
                        else
                        {
                            await locator.WaitForAsync(new LocatorWaitForOptions
                            {
                                State = WaitForSelectorState.Visible,
                                Timeout = 5000
                            });

                            log.AppendLine($"[{DateTime.Now}] Element visible: {step.Selector}");
                        }

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_assertelement.png");
                        break;

                    case "screenshot":
                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_screenshot.png");

                        log.AppendLine($"[{DateTime.Now}] Screenshot gemaakt");
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

            var fullError = ex.ToString();

            log.AppendLine($"[{DateTime.Now}] ERROR FULL:");
            log.AppendLine(fullError);

            Console.WriteLine("ERROR FULL:");
            Console.WriteLine(fullError);

            if (page != null)
            {
                try
                {
                    await SafeScreenshot(page, runFolder, "error.png");
                }
                catch (Exception screenshotEx)
                {
                    log.AppendLine("Screenshot bij error mislukt:");
                    log.AppendLine(screenshotEx.ToString());
                }
            }

            if (browser != null)
            {
                try
                {
                    await browser.CloseAsync();
                }
                catch (Exception closeEx)
                {
                    log.AppendLine("Browser sluiten mislukt:");
                    log.AppendLine(closeEx.ToString());
                }
            }

            return ("Fail", log.ToString(), stopwatch.Elapsed.TotalSeconds, folderName);
        }
    }

    private async Task SafeScreenshot(IPage page, string folder, string fileName)
    {
        try
        {
            var fullPath = Path.Combine(folder, fileName);

            await page.WaitForTimeoutAsync(1000);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fullPath,
                FullPage = true
            });

            Console.WriteLine($"Screenshot opgeslagen: {fullPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Screenshot error: " + ex.Message);
        }
    }
}