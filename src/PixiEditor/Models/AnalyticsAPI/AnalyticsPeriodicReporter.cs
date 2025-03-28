using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;

namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticsPeriodicReporter
{
    private int _sendExceptions = 0;
    private bool _resumeSession;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AnalyticsClient _client;
    private readonly PeriodicPerformanceReporter _performanceReporter;

    private readonly List<AnalyticEvent> _backlog = new();
    private readonly CancellationTokenSource _cancellationToken = new();

    private DateTime lastActivity;

    public static AnalyticsPeriodicReporter? Instance { get; private set; }

    public Guid SessionId { get; private set; }

    public AnalyticsPeriodicReporter(AnalyticsClient client)
    {
        if (Instance != null)
            throw new InvalidOperationException("There's already a AnalyticsReporter present");

        Instance = this;

        _client = client;
        _performanceReporter = new PeriodicPerformanceReporter(this);

        PixiEditorSettings.Analytics.AnalyticsEnabled.ValueChanged += EnableAnalyticsOnValueChanged;
    }

    public void Start(Guid? sessionId)
    {
        if (!PixiEditorSettings.Analytics.AnalyticsEnabled.Value)
            return;

        if (sessionId != null)
        {
            SessionId = sessionId.Value;
            _resumeSession = true;

            _backlog.Add(new AnalyticEvent { Time = DateTime.UtcNow, EventType = AnalyticEventTypes.ResumeSession });
        }

        Task.Run(RunAsync);
        _performanceReporter.StartPeriodicReporting();
    }

    public async Task StopAsync()
    {
        await _cancellationToken.CancelAsync();

        await _client.EndSessionAsync(SessionId).WaitAsync(TimeSpan.FromSeconds(1));
    }

    public void AddEvent(AnalyticEvent value)
    {
        // Don't send startup as it gives invalid results for crash resumed sessions
        if (value.EventType == AnalyticEventTypes.Startup && _resumeSession)
        {
            return;
        }

        Task.Run(() =>
        {
            _semaphore.Wait();

            try
            {
                _backlog.Add(value);
            }
            finally
            {
                _semaphore.Release();
            }
        });
    }

    private async Task RunAsync()
    {
        if (!_resumeSession)
        {
            var createSession = await _client.CreateSessionAsync(_cancellationToken.Token);

            if (!createSession.HasValue)
            {
                return;
            }

            SessionId = createSession.Value;
        }

        Task.Run(RunHeartbeatAsync);

        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_backlog.Any(x => x.ExpectingEndTimeReport))
                    WaitForEndTimes();

                await SendBacklogAsync();

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
            catch (TaskCanceledException) { }
            catch (Exception e)
            {
                await SendExceptionAsync(e);
            }
        }
    }

    private void WaitForEndTimes()
    {
        var totalTimeout = DateTime.Now + TimeSpan.FromSeconds(10);

        foreach (var backlog in _backlog)
        {
            var timeout = totalTimeout - DateTime.Now;

            if (timeout < TimeSpan.Zero)
            {
                break;
            }

            backlog.WaitForEndTime(timeout);
        }
    }

    private async Task SendBacklogAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_backlog.Count == 0)
            {
                return;
            }

            var result = await _client.SendEventsAsync(SessionId, _backlog, _cancellationToken.Token);
            _backlog.Clear();

            if (result == null) _cancellationToken.Cancel();

            lastActivity = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task RunHeartbeatAsync()
    {
        lastActivity = DateTime.UtcNow;

        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatIfNeededAsync();

                await Task.Delay(TimeSpan.FromSeconds(10), _cancellationToken.Token);
            }
            catch (TaskCanceledException) { }
            catch (Exception e)
            {
                await SendExceptionAsync(e);
            }
        }
    }

    private async ValueTask SendHeartbeatIfNeededAsync()
    {
        var timeSinceLastActivity = DateTime.UtcNow - lastActivity;
        if (timeSinceLastActivity.TotalSeconds < 60)
        {
            return;
        }

        var result = await _client.SendHeartbeatAsync(SessionId, _cancellationToken.Token);
        lastActivity = DateTime.UtcNow;

        if (!result)
        {
            await _cancellationToken.CancelAsync();
        }
    }

    private async Task SendExceptionAsync(Exception e)
    {
        if (_sendExceptions > 6)
        {
            await CrashHelper.SendExceptionInfoAsync(e);
            _sendExceptions++;
        }
    }

    private void EnableAnalyticsOnValueChanged(Setting<bool> setting, bool enabled)
    {
        if (enabled)
        {
            Start(null);
        }
        else
        {
            StopAsync();
        }
    }
}
