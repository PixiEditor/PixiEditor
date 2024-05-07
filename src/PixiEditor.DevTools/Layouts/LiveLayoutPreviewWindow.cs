using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.DevTools.Layouts;

public class LiveLayoutPreviewWindow : StatefulElement<LivePreviewWindowState>
{
    public override LivePreviewWindowState CreateState()
    {
        return new();
    }
}
