using PixiEditor.Extensions.Sdk;

namespace PixiEditor.ClosedBeta;

public class ClosedBetaExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        if (Api.Preferences.GetPreference<bool>("ClosedBetaWelcomeShown"))
        {
            return;   
        }
        
        WelcomeMessage welcomeMessage = new();
        var window = Api.WindowProvider.CreatePopupWindow("Welcome to the closed beta!", welcomeMessage);
        welcomeMessage.OnContinue += () =>
        {
            Api.Preferences.UpdatePreference("ClosedBetaWelcomeShown", true);
            window.Close();
        };

        window.Width = 800;
        window.Height = 600;
        
        window.CanResize = false;
        window.CanMinimize = false;
        window.ShowDialog();
    }
}
