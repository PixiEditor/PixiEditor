using Avalonia.Controls;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class AnchorHandle : RectangleHandle
{
    public AnchorHandle(Overlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
    }
}
