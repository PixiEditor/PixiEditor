using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PixiEditor.Helpers;
using PixiEditor.Models.Input;
using Drawie.Numerics;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticsClient : IDisposable
{
    private readonly HttpClient _client = new();

    private readonly JsonSerializerOptions _options = new()
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new KeyCombinationConverter(),
            new VecDConverter(),
            new VersionConverter()
        }
    };

    public AnalyticsClient(string url)
    {
        _client.BaseAddress = new Uri(url);
    }

    public async Task<Guid?> CreateSessionAsync(CancellationToken cancellationToken = default)
    {
        var session = new AnalyticSessionInfo(IOperatingSystem.Current)
        {
            Version = VersionHelpers.GetCurrentAssemblyVersion(),
            BuildId = VersionHelpers.GetBuildId(),
        };
        
        var response = await _client.PostAsJsonAsync("sessions/new", session, _options, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Guid?>(_options, cancellationToken);
        }
        
        if (response.StatusCode is not HttpStatusCode.ServiceUnavailable)
        {
            await ReportInvalidStatusCodeAsync(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
        }
            
        return null;

    }

    public async Task<bool> SendEventsAsync(Guid sessionId, IEnumerable<AnalyticEvent> events,
        CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsJsonAsync($"sessions/{sessionId}/events", events, _options, cancellationToken);
        
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        if (response.StatusCode is not (HttpStatusCode.NotFound or HttpStatusCode.ServiceUnavailable))
        {
            await ReportInvalidStatusCodeAsync(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
        }
            
        return false;
    }

    public async Task<bool> SendHeartbeatAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync($"sessions/{sessionId}/heartbeat", null, cancellationToken);

        return response.IsSuccessStatusCode;
    }

    public async Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync($"sessions/{sessionId}", cancellationToken);
    }

    public async Task SendReportAsync(string jsonText)
    {
        var textContent = new StringContent(jsonText, Encoding.UTF8, MediaTypeNames.Application.Json);
        
        await _client.PostAsync("reports/new", textContent);
    }

    public static string? GetAnalyticsUrl()
    {
        string url = RuntimeConstants.AnalyticsUrl;

        if (url == "${analytics-url}")
        {
            url = null;
            SetDebugUrl(ref url);
        }

        return url;

        [Conditional("DEBUG")]
        static void SetDebugUrl(ref string? url)
        {
            url = Environment.GetEnvironmentVariable("PixiEditorAnalytics");
        }
    }
    
    private static async Task ReportInvalidStatusCodeAsync(HttpStatusCode statusCode, string message)
    {
        await CrashHelper.SendExceptionInfoAsync(new InvalidOperationException($"Invalid status code from analytics API '{statusCode}', message: {message}"));
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

    class VecDConverter : JsonConverter<VecD>
    {
        public override VecD Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, VecD value, JsonSerializerOptions options)
        {
            var invariant = CultureInfo.InvariantCulture;
            writer.WriteStringValue($"{value.X.ToString("F2", invariant)}; {value.Y.ToString("F2", invariant)}");
        }
    }
    
    class VersionConverter : JsonConverter<Version>
    {
        public override Version? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
