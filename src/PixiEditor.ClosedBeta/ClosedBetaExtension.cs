using PixiEditor.Extensions.Sdk;

namespace PixiEditor.ClosedBeta;

public class ClosedBetaExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        WelcomeMessage welcomeMessage = new();
        var window = Api.WindowProvider.CreatePopupWindow("Welcome to the closed beta!", welcomeMessage);
        welcomeMessage.OnContinue += () => window.Close();
        
        window.CanResize = false;
        window.CanMinimize = false;
        window.ShowDialog();
    }
}
