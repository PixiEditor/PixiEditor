using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Enums;

public static class ResizeAnchorEx
{
    public static VecI FindOffsetFor(this ResizeAnchor anchor, VecI imageSize, VecI newImageSize)
    {
        VecI centerOffset = -(VecI)(imageSize / 2f - newImageSize / 2f).Round();
        VecI botRightOffset = newImageSize - imageSize;
        return anchor switch
        {
            ResizeAnchor.TopLeft => VecI.Zero,
            ResizeAnchor.Top => new(centerOffset.X, 0),
            ResizeAnchor.TopRight => new(botRightOffset.X, 0),
            ResizeAnchor.Left => new(0, centerOffset.Y),
            ResizeAnchor.Center => centerOffset,
            ResizeAnchor.Right => new(botRightOffset.X, centerOffset.Y),
            ResizeAnchor.BottomLeft => new(0, botRightOffset.Y),
            ResizeAnchor.Bottom => new(centerOffset.X, botRightOffset.Y),
            ResizeAnchor.BottomRight => botRightOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
        };
    }
}
