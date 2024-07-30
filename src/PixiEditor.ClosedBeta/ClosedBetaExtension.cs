using PixiEditor.Extensions.Sdk;

namespace PixiEditor.ClosedBeta;

public class ClosedBetaExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        Api.WindowProvider.CreatePopupWindow("Welcome to the closed beta!", new WelcomeMessage()).ShowDialog();
    }
}
