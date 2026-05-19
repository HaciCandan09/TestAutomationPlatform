using System.Text.Json;
using TestAutomationPlatform.DTO;

namespace TestAutomationPlatform.Services;

public class ScriptParser
{
    private static readonly HashSet<string> SupportedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "goto",
        "click",
        "fill",
        "press",
        "wait",
        "asserttitle",
        "assertelement",
        "asserturl",
        "asserttext",
        "waitforselector",
        "select",
        "check",
        "uncheck",
        "screenshot"
    };

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public List<TestStep> Parse(string json)
    {
        return JsonSerializer.Deserialize<List<TestStep>>(json, Options) ?? new List<TestStep>();
    }

    public List<string> Validate(string? json)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            errors.Add("Script JSON is verplicht.");
            return errors;
        }

        List<TestStep> steps;

        try
        {
            steps = Parse(json);
        }
        catch (JsonException ex)
        {
            errors.Add($"Ongeldige JSON: {ex.Message}");
            return errors;
        }

        if (steps.Count == 0)
        {
            errors.Add("Het script moet minstens een stap bevatten.");
            return errors;
        }

        for (var index = 0; index < steps.Count; index++)
        {
            var stepNumber = index + 1;
            var step = steps[index];

            if (string.IsNullOrWhiteSpace(step.Action))
            {
                errors.Add($"Stap {stepNumber}: action is verplicht.");
                continue;
            }

            if (!SupportedActions.Contains(step.Action))
            {
                errors.Add($"Stap {stepNumber}: onbekende action '{step.Action}'.");
                continue;
            }

            var action = step.Action.ToLowerInvariant();

            if (RequiresSelector(action) && string.IsNullOrWhiteSpace(step.Selector))
            {
                errors.Add($"Stap {stepNumber}: selector is verplicht voor {action}.");
            }

            if (RequiresValue(action) && string.IsNullOrWhiteSpace(step.Value))
            {
                errors.Add($"Stap {stepNumber}: value is verplicht voor {action}.");
            }

            if (step.Timeout is < 1)
            {
                errors.Add($"Stap {stepNumber}: timeout moet groter zijn dan 0.");
            }
        }

        return errors;
    }

    private static bool RequiresSelector(string action)
    {
        return action is "click" or "fill" or "press" or "assertelement" or "asserttext" or "waitforselector" or "select" or "check" or "uncheck";
    }

    private static bool RequiresValue(string action)
    {
        return action is "goto" or "fill" or "press" or "wait" or "asserttitle" or "asserturl" or "asserttext" or "select";
    }
}
