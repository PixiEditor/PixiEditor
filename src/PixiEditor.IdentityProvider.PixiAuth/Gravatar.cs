using System.Net.Http.Headers;

namespace PixiEditor.IdentityProvider.PixiAuth;

public static class Gravatar
{
    private static HttpClient httpClient = new HttpClient();
    private const string GravatarUrl = "https://www.gravatar.com/";

    public static async Task<string?> GetUsername(string emailHash)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{GravatarUrl}{emailHash}.json");
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PixiEditor", "2.0"));
        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = System.Text.Json.JsonDocument.Parse(content);
            if (json.RootElement.TryGetProperty("entry", out var entry) && entry.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                if (entry.GetArrayLength() > 0)
                {
                    return GetUsernameFromJson(entry[0]);
                }
            }
        }

        return null;
    }

    private static string? GetUsernameFromJson(System.Text.Json.JsonElement entry)
    {
        if (entry.TryGetProperty("preferredUsername", out var username))
        {
            return username.GetString();
        }

        if (entry.TryGetProperty("displayName", out var name))
        {
            return name.GetString();
        }

        return null;
    }
}
