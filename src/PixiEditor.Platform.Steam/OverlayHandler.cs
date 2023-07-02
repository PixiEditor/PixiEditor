using Steamworks;

namespace PixiEditor.Platform.Steam;

public class SteamOverlayHandler
{
    public event Action<bool> ActivateRefreshingElement;
    protected Callback<GameOverlayActivated_t> overlayActivated;

    public SteamOverlayHandler()
    {
        overlayActivated = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);
        InitStartingRefresh();
    }

    private void InitStartingRefresh()
    {
        System.Timers.Timer timer = new(10000);
        timer.Elapsed += (sender, args) =>
        {
            if (SteamUtils.IsOverlayEnabled()) return;

            ActivateRefreshingElement?.Invoke(false);
        };
        timer.Start();
    }

    private void OnOverlayActivated(GameOverlayActivated_t param)
    {
        ActivateRefreshingElement?.Invoke(param.m_bActive == 1);
    }
}
