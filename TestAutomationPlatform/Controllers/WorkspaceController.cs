using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Controllers
{
    [ApiController]
    [Route("api/workspace")]
    public class WorkspaceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WorkspaceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var workspaces = await _context.Workspaces.ToListAsync();
            return Ok(workspaces);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Workspace workspace)
        {
            _context.Workspaces.Add(workspace);
            await _context.SaveChangesAsync();
            return Ok(workspace);
        }
    }
}