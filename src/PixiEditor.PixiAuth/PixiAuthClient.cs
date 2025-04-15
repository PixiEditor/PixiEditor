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
            Dictionary<string, string>? resultDict =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            if (resultDict == null || !resultDict.TryGetValue("sessionId", out string? sessionIdString))
            {
                return null;
            }

            if (Guid.TryParse(sessionIdString, out Guid sessionId))
            {
                return sessionId;
            }
        }

        return null;
    }

    public async Task<string?> TryClaimSessionToken(string email, Guid session)
    {
        Dictionary<string, string> body = new() { { "email", email }, { "sessionId", session.ToString() } };
        var response = await httpClient.GetAsync($"/session/claimToken?userEmail={email}&sessionId={session}");

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            Dictionary<string, string>? resultDict =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            string? token = null;
            if (resultDict != null && resultDict.TryGetValue("token", out token))
            {
                return token;
            }

            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
        }

        return null;
    }
}
