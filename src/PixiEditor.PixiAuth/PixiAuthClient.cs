using System.Net.Http.Json;

namespace PixiEditor.PixiAuth;

public class PixiAuthClient
{
    private HttpClient httpClient;

    public string BaseUrl { get; }

    public PixiAuthClient(string baseUrl)
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<Guid?> GenerateSession(string email)
    {
        var response = await httpClient.PostAsJsonAsync("/session/generateSession", email);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            if (Guid.TryParse(result, out Guid sessionId))
            {
                return sessionId;
            }
        }

        return null;
    }

    public async Task<string?> TryGetSessionToken(string email, Guid session)
    {
        Dictionary<string, string> body = new() { { "email", email }, { "sessionId", session.ToString() } };
        var response = await httpClient.PostAsJsonAsync("/session/getSessionToken", body);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
        }

        return null;
    }
}
