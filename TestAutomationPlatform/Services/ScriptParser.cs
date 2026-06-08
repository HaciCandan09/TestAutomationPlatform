using System.Text.Json;
using TestAutomationPlatform.DTO;

namespace TestAutomationPlatform.Services;

public class ScriptParser
{
    public List<TestStep> Parse(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<TestStep>>(json, options) ?? new List<TestStep>();
    }
}