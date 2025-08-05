using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using PixiEditor.PixiAuth.Exceptions;
using PixiEditor.PixiAuth.Models;

namespace PixiEditor.PixiAuth;

public class PixiAuthClient
{
    private HttpClient httpClient;


    public PixiAuthClient(string baseUrl, string? apiKey)
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(240);
        if (apiKey != null)
        {
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        }
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
        else if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }
        else if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                if (int.TryParse(values.FirstOrDefault(), out int retryAfter))
                {
                    throw new TooManyRequestsException("TOO_MANY_REQUESTS", retryAfter / 1000d);
                }
            }
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("UNAUTHORIZED");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException("FORBIDDEN");
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
        else if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("UNAUTHORIZED");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException("FORBIDDEN");
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
        else if ((int)response.StatusCode >= 500)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("UNAUTHORIZED");
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenException("FORBIDDEN");
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

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                if (int.TryParse(values.FirstOrDefault(), out int retryAfter))
                {
                    throw new TooManyRequestsException("TOO_MANY_REQUESTS", retryAfter / 1000d);
                }
            }
        }
        else if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }
    }

    public async Task<bool> OwnsProduct(string token, string productId)
    {
        HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Get, $"/session/ownsProduct?productId={productId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }

        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            string result = await response.Content.ReadAsStringAsync();
            Dictionary<string, string>? resultDict =
                System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result);
            if (resultDict != null && resultDict.TryGetValue("ownsProduct", out string? ownsProductString))
            {
                if (bool.TryParse(ownsProductString, out bool ownsProduct))
                {
                    return ownsProduct;
                }
            }
        }

        return false;
    }

    public async Task<List<Product>> GetOwnedProducts(string token)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/content/getOwnedProducts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }

        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            string result = await response.Content.ReadAsStringAsync();
            try
            {
                List<Product>? ownedProducts = JsonSerializer.Deserialize<List<Product>>(result);

                return ownedProducts ?? new List<Product>();
            }
            catch (JsonException)
            {
                // Handle JSON parsing error
                throw new BadRequestException("PARSING_FAILED");
            }
        }

        return [];
    }

    public async Task<Stream> DownloadProduct(string token, string productId)
    {
        HttpRequestMessage request =
            new HttpRequestMessage(HttpMethod.Get, $"/content/downloadProduct?productId={productId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(productId);

        var response = await httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }

        if (response.StatusCode >= HttpStatusCode.InternalServerError)
        {
            throw new InternalServerErrorException("INTERNAL_SERVER_ERROR");
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadAsStreamAsync();
            if (result != null)
            {
                return result;
            }
        }

        throw new BadRequestException("DOWNLOAD_FAILED");
    }
}
