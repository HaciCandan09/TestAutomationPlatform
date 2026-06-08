using Microsoft.AspNetCore.Mvc;
using TestAutomationPlatform.Models;
using TestAutomationPlatform.Services;

namespace TestAutomationPlatform.Controllers
{
    public class DefectController : Controller
    {
        private readonly IDefectService _defectService;

        public DefectController(IDefectService defectService)
        {
            _defectService = defectService;
        }

        public async Task<IActionResult> Index()
        {
            var defects = await _defectService.GetAllAsync();
            return View(defects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var defect = await _defectService.GetByIdAsync(id);

            if (defect == null)
                return NotFound();

            return View(defect);
        }

        [HttpGet]
        public IActionResult Create(int runResultId)
        {
            var defect = new Defect
            {
                RunResultId = runResultId,
                Status = DefectStatus.Open,
                Priority = DefectPriority.Middel
            };

            return View(defect);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
    int runResultId,
    string title,
    string? description,
    DefectPriority priority)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError("Title", "Titel is verplicht.");

                var defectModel = new Defect
                {
                    RunResultId = runResultId,
                    Title = title,
                    Description = description,
                    Priority = priority,
                    Status = DefectStatus.Open
                };

                return View(defectModel);
            }

            try
            {
                await _defectService.CreateFromRunResultAsync(runResultId, title, description, priority);

                TempData["SuccessMessage"] = "Defect is succesvol aangemaakt.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                var defectModel = new Defect
                {
                    RunResultId = runResultId,
                    Title = title,
                    Description = description,
                    Priority = priority,
                    Status = DefectStatus.Open
                };

                return View(defectModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, DefectStatus status)
        {
            await _defectService.UpdateStatusAsync(id, status);
            return RedirectToAction("Details", new { id });
        }
    }
}