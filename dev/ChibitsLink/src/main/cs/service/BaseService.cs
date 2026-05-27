using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChibitsLink.main.cs.service;

public abstract class BaseService
{
    protected readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "http://192.168.1.100:3000"; // Default server IP, should be configurable

    protected BaseService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BaseService] HTTP Request Error ({endpoint}): {ex.Message}");
            return default;
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine($"[BaseService] HTTP Timeout ({endpoint}).");
            return default;
        }
        catch (System.Text.Json.JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BaseService] JSON Serialization/Deserialization Error ({endpoint}): {ex.Message}");
            return default;
        }
    }

    protected void SetAuthToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
