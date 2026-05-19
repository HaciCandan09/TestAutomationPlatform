using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    public class DashboardController : Controller
    {
        private const int ResultLimit = 100;
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? environment, string? status, string? search)
        {
            var results = await GetDashboardResults(environment, status, search);
            SetDashboardSummary(results, environment, status, search);

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetResultsPartial(string? environment, string? status, string? search)
        {
            var results = await GetDashboardResults(environment, status, search);
            return PartialView("PartialDataRefresh", results);
        }

        private async Task<List<DashboardViewModel>> GetDashboardResults(string? environment, string? status, string? search)
        {
            var query =
                from rr in _context.RunResults.AsNoTracking()
                join s in _context.Scripts.AsNoTracking() on rr.ScriptId equals s.Id
                join w in _context.Workspaces.AsNoTracking() on s.WorkspaceId equals w.Id
                join ts in _context.TestSuites.AsNoTracking() on s.TestSuiteId equals ts.Id
                join c in _context.Categories.AsNoTracking() on s.CategoryId equals c.Id
                select new DashboardViewModel
                {
                    Id = rr.Id,
                    ScriptId = rr.ScriptId,
                    ScriptName = s.Name,
                    WorkspaceName = w.Name,
                    TestSuiteName = ts.Name,
                    CategoryName = c.Name,
                    Environment = rr.Environment,
                    Status = rr.Status,
                    Log = rr.Log,
                    ScreenshotPath = rr.ScreenshotPath,
                    ExecutionTime = rr.ExecutionTime,
                    ExecutedAt = rr.ExecutedAt
                };

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(x => x.Environment == environment);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.ScriptName.Contains(search));
            }

            return await query
                .OrderByDescending(x => x.ExecutedAt)
                .Take(ResultLimit)
                .ToListAsync();
        }

        private void SetDashboardSummary(List<DashboardViewModel> results, string? environment, string? status, string? search)
        {
            ViewBag.Environment = environment ?? string.Empty;
            ViewBag.Status = status ?? string.Empty;
            ViewBag.Search = search ?? string.Empty;
            ViewBag.ResultLimit = ResultLimit;
            ViewBag.TotalRuns = results.Count;
            ViewBag.PassedRuns = results.Count(x => x.Passed);
            ViewBag.FailedRuns = results.Count(x => x.Failed);
            ViewBag.AverageExecutionTime = results.Count == 0 ? 0 : results.Average(x => x.ExecutionTime);
            ViewBag.LastRunAt = results.Count == 0 ? null : results.Max(x => x.ExecutedAt);
        }
    }
}
