using PixiEditor.Extensions.Sdk;

namespace PixiEditor.ClosedBeta;

public class BetaExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        if (Api.Preferences.GetPreference<bool>("BetaWelcomeShown"))
        {
            return;
        }

        WelcomeMessage welcomeMessage = new();
        var window = Api.WindowProvider.CreatePopupWindow("Welcome to the PixiEditor 2.0 beta!", welcomeMessage);
        welcomeMessage.OnContinue += () =>
        {
            Api.Preferences.UpdatePreference("BetaWelcomeShown", true);
            window.Close();
        };

        window.Width = 800;
        window.Height = 600;

        window.CanResize = false;
        window.CanMinimize = false;
        window.ShowDialog();
    }
}
