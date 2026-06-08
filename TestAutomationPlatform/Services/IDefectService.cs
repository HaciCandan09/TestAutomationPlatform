using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Services
{
    public interface IDefectService
    {
        Task<List<Defect>> GetAllAsync();
        Task<Defect?> GetByIdAsync(int id);
        Task<Defect> CreateFromRunResultAsync(int runResultId, string title, string? description, DefectPriority priority);
        Task UpdateStatusAsync(int defectId, DefectStatus status);
    }
}
