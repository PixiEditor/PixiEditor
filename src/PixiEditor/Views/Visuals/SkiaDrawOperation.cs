using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;

namespace PixiEditor.Views.Visuals;

internal abstract class SkiaDrawOperation : ICustomDrawOperation
{
    public Rect Bounds { get; set; }

    public SkiaDrawOperation(Rect dirtyBounds)
    {
        Bounds = dirtyBounds;
    }

    public abstract bool Equals(ICustomDrawOperation? other);

    public virtual void Dispose()
    {

    }

    void IDisposable.Dispose()
    {
        Dispose();
    }

    public virtual bool HitTest(Point p) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (!context.TryGetFeature(out ISkiaSharpApiLeaseFeature leaseFeature))
        {
            throw new InvalidOperationException("SkiaSharp API lease feature is not available.");
        }

        using var lease = leaseFeature.Lease();

        Render(lease);
    }

    public abstract void Render(ISkiaSharpApiLease lease);
}
