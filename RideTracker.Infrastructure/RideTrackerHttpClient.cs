using Firebase.Auth;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace RideTracker.Infrastructure;

public class RideTrackerHttpClient
{
    private readonly DbLogger<RideTrackerHttpClient> _logger;
    private readonly FirebaseAuthClient _authClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public RideTrackerHttpClient(DbLogger<RideTrackerHttpClient> logger, FirebaseAuthClient authClient, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _authClient = authClient;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request)
    {
        _logger.LogInformation($"Sending {request.Method} request to {request.RequestUri}");
        if(_authClient.User is null)
        {
            _logger.LogError("User is not signed in. Cannot send request.");
            return default;
        }

        var token = await _authClient.User.GetIdTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = null;
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ride-tracker");
            response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation($"Request to {request.RequestUri} succeeded with status code {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Request to {request.RequestUri} failed.");
            throw;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(responseContent) 
            ? default 
            : JsonConvert.DeserializeObject<T>(responseContent);
    }

    public async Task<T?> GetAsync<T>(string requestUri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync<T>(request);
    }

    public async Task<T?> PostAsync<T>(string requestUri, object content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
        };
        return await SendAsync<T>(request);
    }
}
