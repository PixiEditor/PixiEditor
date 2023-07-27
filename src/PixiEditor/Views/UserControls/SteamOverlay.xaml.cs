using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PixiEditor.Views.UserControls;

public partial class SteamOverlay : UserControl
{
    private DispatcherTimer _timer;
    private DispatcherTimer _fadeTimer;
    public SteamOverlay()
    {
        InitializeComponent();
        CreateRefreshTimer();
    }

    private void CreateFadeTimer()
    {
        StopFadeTimer();
        _fadeTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(1f) };
        _fadeTimer.Tick += FadeOut;
    }

    private void FadeOut(object sender, EventArgs eventArgs)
    {
        RemoveTimer();
        Visibility = Visibility.Collapsed;
        StopFadeTimer();
    }

    private void CreateRefreshTimer()
    {
        RemoveTimer();
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16.6f) };
        _timer.Tick += Refresh;
    }

    private void RemoveTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= Refresh;
            _timer = null;
        }
    }

    public void Activate()
    {
        StopFadeTimer();
        CreateRefreshTimer();
        Visibility = Visibility.Visible;
        _timer.Start();
    }

    public void Deactivate()
    {
        CreateFadeTimer();
        _fadeTimer.Start();
    }

    private void StopFadeTimer()
    {
        if (_fadeTimer != null)
        {
            _fadeTimer.Stop();
            _fadeTimer.Tick -= FadeOut;
            _fadeTimer = null;
        }
    }

    private void Refresh(object sender, EventArgs e)
    {
        refresher.InvalidateVisual();
    }
}

