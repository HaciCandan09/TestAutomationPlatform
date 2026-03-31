using Microsoft.AspNetCore.Mvc;
using TestAutomationPlatform.Models;
using TestAutomationPlatform.Repository;

[ApiController]
[Route("api/script")]
public class ScriptController : ControllerBase
{
    private readonly IScriptRepository _repo;

    public ScriptController(IScriptRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scripts = await _repo.GetAll();
        return Ok(scripts);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Script script)
    {
        await _repo.Add(script);
        return Ok(script);
    }
}