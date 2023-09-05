using Avalonia.Controls;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class AnchorHandle : RectangleHandle
{
    public AnchorHandle(Control owner, VecD position) : base(owner, position)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
    }
}
