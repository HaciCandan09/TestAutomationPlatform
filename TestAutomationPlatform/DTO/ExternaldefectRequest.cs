using System.Text.Json.Serialization;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.DTO;

public class ExternalDefectRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "Test Automation";

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("runResultId")]
    public int RunResultId { get; set; }

    [JsonPropertyName("dtCreate")]
    public DateTime DtCreate { get; set; }

    [JsonPropertyName("dtModified")]
    public DateTime DtModified { get; set; }

    public static ExternalDefectRequest FromDefect(Defect defect)
    {
        return new ExternalDefectRequest
        {
            Id = 0,
            Title = defect.Title,
            Description = defect.Description ?? string.Empty,
            Type = "Test Automation",
            Priority = (int)defect.Priority,
            Status = (int)defect.Status,
            RunResultId = defect.RunResultId,
            DtCreate = defect.CreatedAt,
            DtModified = defect.UpdatedAt ?? defect.CreatedAt
        };
    }
}