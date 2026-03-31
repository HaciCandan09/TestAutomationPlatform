using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    [ApiController]
    [Route("api/testsuite")]
    public class TestSuiteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestSuiteController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suites = await _context.TestSuites
                .Include(ts => ts.Workspace)
                .ToListAsync();

            return Ok(suites);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TestSuite suite)
        {
            _context.TestSuites.Add(suite);
            await _context.SaveChangesAsync();
            return Ok(suite);
        }
    }
}