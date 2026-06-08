using Microsoft.EntityFrameworkCore;
using TestAutomationPlatform.Data;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Services
{
    public class DefectService : IDefectService
    {
        private readonly AppDbContext _context;

        public DefectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Defect>> GetAllAsync()
        {
            return await _context.Defects
                .Include(d => d.RunResult)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Defect?> GetByIdAsync(int id)
        {
            return await _context.Defects
                .Include(d => d.RunResult)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Defect> CreateFromRunResultAsync(
            int runResultId,
            string title,
            string? description,
            DefectPriority priority)
        {
            var result = await _context.RunResults
                .Include(r => r.Defect)
                .FirstOrDefaultAsync(r => r.Id == runResultId);

            if (result == null)
                throw new Exception("Testresultaat niet gevonden.");

            if (!result.Status.Equals("Fail", StringComparison.OrdinalIgnoreCase) &&
     !result.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase) &&
     !result.Status.Equals("Error", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Een defect kan alleen worden aangemaakt bij een gefaald testresultaat.");
            }

            if (result.Defect != null)
                throw new Exception("Voor dit testresultaat bestaat al een defect.");

            var defect = new Defect
            {
                RunResultId = runResultId,
                Title = title,
                Description = description,
                Priority = priority,
                Status = DefectStatus.Open,
                CreatedAt = DateTime.Now
            };

            _context.Defects.Add(defect);
            await _context.SaveChangesAsync();

            return defect;
        }

        public async Task UpdateStatusAsync(int defectId, DefectStatus status)
        {
            var defect = await _context.Defects.FindAsync(defectId);

            if (defect == null)
                throw new Exception("Defect niet gevonden.");

            defect.Status = status;
            defect.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }
}