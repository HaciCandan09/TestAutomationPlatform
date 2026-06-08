using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Repository
{
    public class ScriptRepository : IScriptRepository
    {
        private readonly AppDbContext _context;

        public ScriptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Script>> GetAll()
        {
            return await _context.Scripts
                .Include(s => s.Workspace)
                .Include(s => s.TestSuite)
                .Include(s => s.Category)
                .ToListAsync();
        }

        public async Task<Script> GetById(int id)
        {
            return await _context.Scripts
                .Include(s => s.Workspace)
                .Include(s => s.TestSuite)
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task Add(Script script)
        {
            _context.Scripts.Add(script);
            await _context.SaveChangesAsync();
        }
    }
}