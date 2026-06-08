using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;
using TestAutomationPlatform.Services;

namespace TestAutomationPlatform.Controllers;

public class ScheduledRunController : Controller
{
    private readonly AppDbContext _context;

    public ScheduledRunController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await BuildViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduledRunViewModel viewModel)
    {
        var scriptExists = await _context.Scripts
            .AnyAsync(s => s.Id == viewModel.ScriptId);

        if (!scriptExists)
        {
            TempData["ErrorMessage"] = "Selecteer een geldig script.";
            return RedirectToAction(nameof(Index));
        }

        var cronExpression = CronHelper.ToCronExpression(viewModel.Interval);
        var description = CronHelper.ToDescription(viewModel.Interval);

        var scheduledRun = new ScheduledRun
        {
            ScriptId = viewModel.ScriptId,
            Environment = viewModel.Environment,
            CronExpression = cronExpression,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.ScheduledRuns.Add(scheduledRun);
        await _context.SaveChangesAsync();

        RecurringJob.AddOrUpdate<RunService>(
            $"scheduled-run-{scheduledRun.Id}",
            service => service.ExecuteScheduledScript(scheduledRun.ScriptId, scheduledRun.Environment),
            scheduledRun.CronExpression
        );

        TempData["SuccessMessage"] = "Geplande run is succesvol aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var scheduledRun = await _context.ScheduledRuns.FindAsync(id);

        if (scheduledRun == null)
        {
            TempData["ErrorMessage"] = "Geplande run niet gevonden.";
            return RedirectToAction(nameof(Index));
        }

        RecurringJob.RemoveIfExists($"scheduled-run-{scheduledRun.Id}");

        _context.ScheduledRuns.Remove(scheduledRun);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Geplande run is verwijderd.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ScheduledRunViewModel> BuildViewModel()
    {
        return new ScheduledRunViewModel
        {
            ScheduledRuns = await _context.ScheduledRuns
                .Include(sr => sr.Script)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToListAsync(),

            Scripts = await _context.Scripts
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync()
        };
    }
}