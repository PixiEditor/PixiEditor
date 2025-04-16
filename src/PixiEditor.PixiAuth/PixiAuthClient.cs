using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PixiEditor.PixiAuth.Exceptions;

namespace PixiEditor.PixiAuth;

public class PixiAuthClient
{
    private HttpClient httpClient;

    public string BaseUrl { get; }

    public PixiAuthClient(string baseUrl)
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        // TODO: Update expiration date locally
        // TODO: Add error code handling
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
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }
        else if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
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
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }
        else if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }

        return null;
    }

    /// <summary>
    ///     /// Refreshes the session token.
    /// </summary>
    /// <param name="userSessionId">Id of the session.</param>
    /// <param name="userSessionToken">Authentication token.</param>
    /// <returns>Token if successful, null otherwise.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the session is not valid.</exception>
    public async Task<string?> RefreshToken(Guid userSessionId, string userSessionToken)
    {
        if (string.IsNullOrEmpty(userSessionToken))
        {
            return null;
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/session/refreshToken");
        request.Content = JsonContent.Create(new SessionModel(userSessionId, userSessionToken));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userSessionToken);

        var response = await httpClient.SendAsync(request);

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
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException("SESSION_NOT_VALID");
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }
        else if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }

        return null;
    }

    public async Task Logout(Guid userSessionId, string userSessionToken)
    {
        if (string.IsNullOrEmpty(userSessionToken))
        {
            return;
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/session/logout");
        string sessionId = userSessionId.ToString(); // Name is important here, do not change!
        request.Content = JsonContent.Create(sessionId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userSessionToken);

        await httpClient.SendAsync(request);
    }

    public async Task ResendActivation(string userEmail, Guid userSessionId)
    {
        if (string.IsNullOrEmpty(userEmail))
        {
            return;
        }

        var response = await httpClient.PostAsJsonAsync("/session/resendActivation",
            new ResendActivationModel(userEmail, userSessionId));

        if (!response.IsSuccessStatusCode)
        {
            Dictionary<string, object> responseData =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                    await response.Content.ReadAsStringAsync());
            if (responseData != null && responseData.TryGetValue("error", out object? error))
            {
                if (error is string errorString and "TOO_MANY_REQUESTS")
                {
                    if (responseData.TryGetValue("timeLeft", out object? timeLeft))
                    {
                        if (timeLeft is double timeLeftDouble)
                        {
                            throw new TooManyRequestsException(errorString, timeLeftDouble);
                        }
                    }

                    throw new BadRequestException(errorString);
                }
            }
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
            }
        }
    }
}
