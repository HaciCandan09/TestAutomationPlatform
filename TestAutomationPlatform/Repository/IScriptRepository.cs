using TestAutomationPlatform.Models;

public interface IScriptRepository
{
    Task<List<Script>> GetAll();
    Task<Script?> GetById(int id);
    Task Add(Script script);
}
