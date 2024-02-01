using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.DevTools.Layouts;

public class LiveLayoutPreviewWindow : StatefulElement<LivePreviewWindowState>
{
    public override LivePreviewWindowState CreateState()
    {
        return new();
    }
}
