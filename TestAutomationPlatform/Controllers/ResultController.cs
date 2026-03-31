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

        // 🔥 Alle resultaten
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _context.RunResults
                .OrderByDescending(r => r.ExecutedAt)
                .ToListAsync();

            return Ok(results);
        }

        // 🔥 Resultaten per script
        [HttpGet("script/{scriptId}")]
        public async Task<IActionResult> GetByScript(int scriptId)
        {
            var results = await _context.RunResults
                .Where(r => r.ScriptId == scriptId)
                .OrderByDescending(r => r.ExecutedAt)
                .ToListAsync();

            return Ok(results);
        }

        // 🔥 Alleen laatste per script (dashboard 🔥)
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            var results = await _context.RunResults
                .GroupBy(r => r.ScriptId)
                .Select(g => g.OrderByDescending(r => r.ExecutedAt).First())
                .ToListAsync();

            return Ok(results);
        }
    }
}