using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;

namespace TestAutomationPlatform.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ResultController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? environment, string? status, int take = 100)
        {
            take = Math.Clamp(take, 1, 500);

            var query = _context.RunResults.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(r => r.Environment == environment);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var results = await query
                .OrderByDescending(r => r.ExecutedAt)
                .Take(take)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("script/{scriptId:int}")]
        public async Task<IActionResult> GetByScript(int scriptId, int take = 100)
        {
            take = Math.Clamp(take, 1, 500);

            var results = await _context.RunResults
                .AsNoTracking()
                .Where(r => r.ScriptId == scriptId)
                .OrderByDescending(r => r.ExecutedAt)
                .Take(take)
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var results = await _context.RunResults
                .AsNoTracking()
                .GroupBy(r => r.ScriptId)
                .Select(g => g.OrderByDescending(r => r.ExecutedAt).First())
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(string? environment)
        {
            var query = _context.RunResults.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(environment))
            {
                query = query.Where(r => r.Environment == environment);
            }

            var total = await query.CountAsync();
            var passed = await query.CountAsync(r => r.Status == "Pass");
            var failed = await query.CountAsync(r => r.Status == "Fail");
            var lastRunAt = await query.OrderByDescending(r => r.ExecutedAt).Select(r => (DateTime?)r.ExecutedAt).FirstOrDefaultAsync();
            var averageExecutionTime = total == 0 ? 0 : await query.AverageAsync(r => r.ExecutionTime);

            return Ok(new
            {
                environment = string.IsNullOrWhiteSpace(environment) ? "All" : environment,
                total,
                passed,
                failed,
                passRate = total == 0 ? 0 : Math.Round((double)passed / total * 100, 1),
                averageExecutionTime = Math.Round(averageExecutionTime, 2),
                lastRunAt
            });
        }
    }
}
