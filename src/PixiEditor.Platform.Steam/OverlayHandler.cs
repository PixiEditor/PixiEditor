using Steamworks;

namespace PixiEditor.Platform.Steam;

public class SteamOverlayHandler
{
    public event Action<bool> ActivateRefreshingElement;
    protected Callback<GameOverlayActivated_t> overlayActivated;

    private bool _isOverlayActive;

    public SteamOverlayHandler()
    {
        overlayActivated = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);
        InitStartingRefresh();
    }

    private void InitStartingRefresh()
    {
        System.Timers.Timer timer = new(11000);
        timer.Elapsed += (sender, args) =>
        {
            if (_isOverlayActive) return;

            ActivateRefreshingElement?.Invoke(false);
            timer.Stop();
        };
        timer.Start();
    }

    private void OnOverlayActivated(GameOverlayActivated_t param)
    {
        _isOverlayActive = param.m_bActive == 1;
        ActivateRefreshingElement?.Invoke(param.m_bActive == 1);
    }
}
