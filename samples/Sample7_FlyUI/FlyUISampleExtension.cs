using PixiEditor.Extensions.Sdk;

namespace FlyUISample;

public class FlyUiSampleExtension : PixiEditorExtension
{
    public override void OnInitialized()
    {
        WindowContentElement content = new WindowContentElement();
        var popup = Api.WindowProvider.CreatePopupWindow("Sample Window", content);
        content.Window = popup;

        popup.Width = 800;
        popup.Height = 720;
        
        popup.Show();
    }
}