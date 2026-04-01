using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var results = await (
                from rr in _context.RunResults
                join s in _context.Scripts on rr.ScriptId equals s.Id
                join w in _context.Workspaces on s.WorkspaceId equals w.Id
                join ts in _context.TestSuites on s.TestSuiteId equals ts.Id
                join c in _context.Categories on s.CategoryId equals c.Id
                orderby rr.ExecutedAt descending
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
                }
            )
            .Take(50)
            .ToListAsync();

            return View(results);
        }
    }
}