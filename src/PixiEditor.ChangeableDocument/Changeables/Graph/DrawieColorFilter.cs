using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public sealed class DrawieColorFilter(Filter previous, ColorFilter filter) : Filter(previous), IDisposable
{
    private bool willDipose;
    private Paint _paint = new() { ColorFilter = filter, BlendMode = BlendMode.Src };

    protected override void DoApply(DrawingSurface surface)
    {
        using var snapshot = surface.Snapshot();
        surface.Canvas.DrawImage(snapshot, 0, 0, _paint);

        if (willDipose)
        {
            _paint.Dispose();
            filter.Dispose();
        }
    }

    public void Dispose()
    {
        willDipose = true;
    }
}
