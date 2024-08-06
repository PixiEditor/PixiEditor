using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using PixiEditor.Helpers;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticsClient
{
    private readonly HttpClient _client = new();
    private readonly JsonSerializerOptions _options = new() { Converters = { new JsonStringEnumConverter(), new KeyCombinationConverter() } };

    public AnalyticsClient(string url)
    {
        _client.BaseAddress = new Uri(url);
    }

    public async Task<Guid?> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"init-session?version={VersionHelpers.GetCurrentAssemblyVersion()}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Guid?>(_options, cancellationToken);
        }

        if (response.StatusCode is not HttpStatusCode.ServiceUnavailable)
        {
            await ReportInvalidStatusCodeAsync(response.StatusCode);
        }
            
        return null;

    }

    public async Task<string?> SendEventsAsync(Guid sessionId, IEnumerable<AnalyticEvent> events,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync($"post-events?id={sessionId}", events, _options, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        if (response.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.ServiceUnavailable))
        {
            await ReportInvalidStatusCodeAsync(response.StatusCode);
        }
            
        return null;
    }

    public async Task<bool> SendHeartbeatAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync($"heartbeat?id={sessionId}", null, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _client.PostAsync($"end-session?id={sessionId}", null, cancellationToken);
    }

    private static async Task ReportInvalidStatusCodeAsync(HttpStatusCode statusCode)
    {
        await CrashHelper.SendExceptionInfoToWebhookAsync(new InvalidOperationException($"Invalid status code from analytics API '{statusCode}'"));
    }

    class KeyCombinationConverter : JsonConverter<KeyCombination>
    {
        public override KeyCombination Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, KeyCombination value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
