using Microsoft.AspNetCore.Mvc;

namespace TestAutomationPlatform.Controllers
{
    public class ScreenshotsController : Controller
    {
        public IActionResult ViewRun(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return NotFound("Geen screenshotpad opgegeven.");
            }

            var screenshotsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            var fullPath = Path.Combine(screenshotsRoot, path);

            if (!Directory.Exists(fullPath))
            {
                return NotFound($"Screenshot map niet gevonden: {fullPath}");
            }

            var files = Directory.GetFiles(fullPath)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .ToList()!;

            ViewBag.Path = path;
            return View(files);
        }

        
        public IActionResult RunImages(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Content("<p>Geen screenshot pad opgegeven.</p>", "text/html");
            }

            var screenshotsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            var fullPath = Path.Combine(screenshotsRoot, path);

            if (!Directory.Exists(fullPath))
            {
                return Content("<p>Screenshot map niet gevonden.</p>", "text/html");
            }

            var files = Directory.GetFiles(fullPath)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .ToList()!;

            ViewBag.Path = path;
            return PartialView("_RunImagesPopup", files);
        }
    }
}