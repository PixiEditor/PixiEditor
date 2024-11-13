using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public sealed class DrawieImageFilter(Filter previous, ImageFilter filter) : Filter(previous)
{
    private Paint _paint = new() { ImageFilter = filter, BlendMode = BlendMode.Src };

    protected override void DoApply(DrawingSurface surface)
    {
        using var snapshot = surface.Snapshot();
        surface.Canvas.DrawImage(snapshot, 0, 0, _paint);
    }

    public void SetImageFilter(ImageFilter imageFilter)
    {
        
    }

    public void Dispose()
    {
        _paint.Dispose();
        filter.Dispose();
    }
}
