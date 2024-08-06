using PixiEditor.Helpers;

namespace PixiEditor.Models.AnalyticsAPI;

public class AnalyticsPeriodicReporter
{
    private int _sendExceptions = 0;
    
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly AnalyticsClient _client;
    
    private readonly List<AnalyticEvent> _backlog = new();
    private readonly CancellationTokenSource _cancellationToken = new();

    private DateTime lastActivity;

    public static AnalyticsPeriodicReporter Instance { get; private set; }

    public Guid SessionId { get; private set; }
    
    public AnalyticsPeriodicReporter(AnalyticsClient client)
    {
        if (Instance != null)
            throw new InvalidOperationException("There's already a AnalyticsReporter present");

        Instance = this;
        
        _client = client;
    }

    public void Start()
    {
        Task.Run(RunAsync);
    }

    public async Task StopAsync()
    {
        _cancellationToken.Cancel();

        await _client.EndSessionAsync(SessionId).WaitAsync(TimeSpan.FromSeconds(1));
    }

    public void AddEvent(AnalyticEvent value)
    {
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
        var createSession = await _client.CreateSessionAsync(_cancellationToken.Token);

        if (!createSession.HasValue)
        {
            return;
        }

        SessionId = createSession.Value;

        Task.Run(RunHeartbeatAsync);

        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
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
            _cancellationToken.Cancel();
        }
    }

    private async Task SendExceptionAsync(Exception e)
    {
        if (_sendExceptions > 6)
        {
            await CrashHelper.SendExceptionInfoToWebhookAsync(e);
            _sendExceptions++;
        }
    }
}
