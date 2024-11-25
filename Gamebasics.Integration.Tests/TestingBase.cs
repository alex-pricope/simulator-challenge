using System.Text;
using System.Text.Json;

namespace Gamebasics.Integration.Tests;

public class TestingBase
{
    private readonly HttpClient _httpClient;
    protected const string ApiRoot = "http://localhost:5054";
    protected const string SimulationsRoute = "/api/v1/simulate/groups";

    protected TestingBase()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiRoot)
        };
    }

    protected async Task<HttpResponseMessage> DoPostRequest(string apiPath, object requestDto)
    {
        var requestBody = new StringContent(JsonSerializer.Serialize(requestDto), Encoding.UTF8, "application/json");
        var apiResponse = await _httpClient.PostAsync(apiPath, requestBody);

        return apiResponse;
    }
}