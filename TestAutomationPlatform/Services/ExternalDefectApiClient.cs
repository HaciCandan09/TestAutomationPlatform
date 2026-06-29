using System.Net.Http.Json;
using TestAutomationPlatform.DTO;
using TestAutomationPlatform.Models;

namespace TestAutomationPlatform.Services;

public class ExternalDefectApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExternalDefectApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task AddDefectAsync(Defect defect)
    {
        var url = _configuration["ExternalDefectApi:AddDefectUrl"];

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new Exception("ExternalDefectApi:AddDefectUrl ontbreekt in appsettings.json.");
        }

        var request = ExternalDefectRequest.FromDefect(defect);

        var response = await _httpClient.PostAsJsonAsync(url, request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Externe defect API fout: {response.StatusCode} - {error}");
        }
    }
}