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



    public async Task<(string status, string log, double time)> RunScript(string scriptJson)
    {
        //throw new Exception("TEST - KOM IK HIER?");

        Console.WriteLine("RunScript gestart!");

        var stopwatch = Stopwatch.StartNew();
        var log = new StringBuilder();

        // ✅ JUIST PAD (altijd goed)
        var basePath = Directory.GetCurrentDirectory();
        var screenshotsPath = Path.Combine(basePath, "Screenshots");
        Directory.CreateDirectory(screenshotsPath);

        var runFolder = Path.Combine(screenshotsPath, $"run_{DateTime.Now.Ticks}");
        Directory.CreateDirectory(runFolder);

        // ✅ TEST FILE (altijd zichtbaar)
        File.WriteAllText(Path.Combine(runFolder, "test.txt"), "werkt");

        IPage? page = null;

        try
        {
            var steps = _parser.Parse(scriptJson);
            Console.WriteLine($"Aantal steps: {steps.Count}");

            using var playwright = await Playwright.CreateAsync();

            // 🔥 Zet tijdelijk false om te zien of browser opent
            var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

            page = await browser.NewPageAsync();

            // 🔥 HARDE TEST (BELANGRIJK)
            await page.GotoAsync("https://google.com");

            await page.ScreenshotAsync(new()
            {
                Path = Path.Combine(runFolder, "test.png")
            });

            Console.WriteLine("Test screenshot gemaakt!");

            int stepIndex = 0;

            foreach (var step in steps)
            {
                stepIndex++;
                Console.WriteLine($"Stap: {step.Action}");

                switch (step.Action.ToLower())
                {
                    case "goto":
                        await page.GotoAsync(step.Value);
                        log.AppendLine($"[{DateTime.Now}] Navigated to {step.Value}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_goto.png");
                        break;

                    case "click":
                        await page.ClickAsync(step.Selector);
                        log.AppendLine($"[{DateTime.Now}] Clicked {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_click.png");
                        break;

                    case "fill":
                        await page.FillAsync(step.Selector, step.Value);
                        log.AppendLine($"[{DateTime.Now}] Filled {step.Selector}");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_fill.png");
                        break;

                    case "asserttitle":
                        var title = await page.TitleAsync();

                        if (!title.Contains(step.Value))
                            throw new Exception($"Title mismatch: {title}");

                        log.AppendLine($"[{DateTime.Now}] Title assertion passed");

                        await SafeScreenshot(page, runFolder, $"step_{stepIndex}_assert.png");
                        break;
                }
            }

            stopwatch.Stop();
            return ("Pass", log.ToString(), stopwatch.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            log.AppendLine($"[{DateTime.Now}] ERROR: {ex.Message}");
            Console.WriteLine("ERROR: " + ex.Message);

            if (page != null)
            {
                await SafeScreenshot(page, runFolder, "error.png");
            }

            return ("Fail", log.ToString(), stopwatch.Elapsed.TotalSeconds);
        }
    }

    // 🔥 VEILIGE SCREENSHOT METHOD (enterprise style)
    private async Task SafeScreenshot(IPage page, string folder, string fileName)
    {
        try
        {
            var fullPath = Path.Combine(folder, fileName);

            await page.ScreenshotAsync(new()
            {
                Path = fullPath
            });

            Console.WriteLine($"Screenshot opgeslagen: {fullPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Screenshot error: " + ex.Message);
        }
    }



}