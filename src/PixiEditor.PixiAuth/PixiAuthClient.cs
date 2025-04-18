using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<(string? token, DateTime? expirationDate)> TryClaimSessionToken(Guid session)
    {
        var response = await httpClient.GetAsync($"/session/claimToken?sessionId={session}");

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            Dictionary<string, string>? resultDict =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            string? token = null;
            DateTime? expirationDate = null;
            if (resultDict != null && resultDict.TryGetValue("token", out token))
            {
                DateTime? expiration = null;
                if (resultDict.TryGetValue("expirationDate", out string? expirationString))
                {
                    if (DateTime.TryParse(expirationString, out DateTime expirationDateValue))
                    {
                        expirationDate = expirationDateValue;
                    }
                }

                return (token, expirationDate);
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

        return (null, null);
    }

    /// <summary>
    ///     /// Refreshes the session token.
    /// </summary>
    /// <param name="userSessionId">Id of the session.</param>
    /// <param name="sessionToken">Authentication token.</param>
    /// <returns>Token if successful, null otherwise.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the session is not valid.</exception>
    public async Task<(string? token, DateTime? expirationDate)> RefreshToken(string sessionToken)
    {
        if (string.IsNullOrEmpty(sessionToken))
        {
            return (null, null);
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/session/refreshToken");
        request.Content = JsonContent.Create(sessionToken); // Name is important here, do not change!
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            Dictionary<string, string>? resultDict =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            string? token = null;
            if (resultDict != null && resultDict.TryGetValue("token", out token))
            {
                DateTime? expirationDate = null;
                if (resultDict.TryGetValue("expirationDate", out string? expirationString))
                {
                    if (DateTime.TryParse(expirationString, out DateTime expirationDateValue))
                    {
                        expirationDate = expirationDateValue;
                    }
                }

                return (token, expirationDate);
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

        return (null, null);
    }

    public async Task Logout(string userSessionToken)
    {
        if (string.IsNullOrEmpty(userSessionToken))
        {
            return;
        }

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/session/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userSessionToken);

        await httpClient.SendAsync(request);
    }

    public async Task ResendActivation(Guid userSessionId)
    {
        var response = await httpClient.PostAsJsonAsync("/session/resendActivation", userSessionId);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            try
            {
                Dictionary<string, object> responseData =
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
                if (responseData != null && responseData.TryGetValue("error", out object? error))
                {
                    if (error is JsonElement errorElement)
                    {
                        error = errorElement.GetString();
                    }


                    if (error is string errorString and "TOO_MANY_REQUESTS")
                    {
                        if (responseData.TryGetValue("timeLeft", out object? timeLeft))
                        {
                            if (timeLeft is JsonElement timeLeftElement)
                            {
                                timeLeft = timeLeftElement.GetDouble();
                            }

                            if (timeLeft is double timeLeftDouble)
                            {
                                double seconds = double.Round(timeLeftDouble / 1000);
                                throw new TooManyRequestsException(errorString, seconds);
                            }
                        }

                        throw new BadRequestException(errorString);
                    }
                }
            }
            catch (JsonException)
            {
                // Handle JSON parsing error
                throw new BadRequestException(responseString);
            }
        }
        else if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }
    }
}
