using Microsoft.AspNetCore.Mvc;

namespace TestAutomationPlatform.Controllers
{
    public class ScreenshotsController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public ScreenshotsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult ViewRun(string path)
        {
            if (!TryGetRunFolder(path, out var fullPath))
            {
                return NotFound("Screenshot map niet gevonden.");
            }

            ViewBag.Path = path;
            return View(GetImageFiles(fullPath));
        }

        public IActionResult RunImages(string path)
        {
            if (!TryGetRunFolder(path, out var fullPath))
            {
                return PartialView("_RunImagesPopup", new List<string>());
            }

            ViewBag.Path = path;
            return PartialView("_RunImagesPopup", GetImageFiles(fullPath));
        }

        private bool TryGetRunFolder(string? path, out string fullPath)
        {
            fullPath = string.Empty;

            if (string.IsNullOrWhiteSpace(path) || path.Contains("..") || Path.IsPathRooted(path))
            {
                return false;
            }

            var screenshotsRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "Screenshots"));
            var requestedPath = Path.GetFullPath(Path.Combine(screenshotsRoot, path));

            if (!requestedPath.StartsWith(screenshotsRoot, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(requestedPath))
            {
                return false;
            }

            fullPath = requestedPath;
            return true;
        }

        private static List<string> GetImageFiles(string fullPath)
        {
            return Directory.GetFiles(fullPath)
                .Select(Path.GetFileName)
                .Where(file => !string.IsNullOrWhiteSpace(file))
                .OrderBy(file => file)
                .ToList()!;
        }
    }
}
