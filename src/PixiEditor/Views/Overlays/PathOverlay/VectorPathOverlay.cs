using Avalonia;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.Drawables;
using PixiEditor.Views.Overlays.Handles;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.PathOverlay;

public class VectorPathOverlay : Overlay
{
    public static readonly StyledProperty<VectorPath> PathProperty =
        AvaloniaProperty.Register<VectorPathOverlay, VectorPath>(
            nameof(Path));

    public VectorPath Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    private DashedStroke dashedStroke = new DashedStroke();

    private List<AnchorHandle> pointsHandles = new List<AnchorHandle>();

    protected override void ZoomChanged(double newZoom)
    {
        dashedStroke.UpdateZoom((float)newZoom);
    }
    
    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (Path is null)
        {
            return;
        }

        dashedStroke.Draw(context, Path);
        var points = Path.Points;

        AdjustHandles(points);
        RenderHandles(context, points);
    }

    private void RenderHandles(Canvas context, VecF[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            pointsHandles[i].Position = new VecD(points[i].X, points[i].Y);
            pointsHandles[i].Draw(context);
        }
    }

    private void AdjustHandles(VecF[] points)
    {
        if (pointsHandles.Count != points.Length)
        {
            if (pointsHandles.Count > points.Length)
            {
                pointsHandles.RemoveRange(points.Length, pointsHandles.Count - points.Length);
                Handles.RemoveRange(points.Length, Handles.Count - points.Length);
            }
            else
            {
                for (int i = pointsHandles.Count; i < points.Length; i++)
                {
                    pointsHandles.Add(new AnchorHandle(this));
                    AddHandle(pointsHandles[i]);
                }
            }
        }
    }
}
