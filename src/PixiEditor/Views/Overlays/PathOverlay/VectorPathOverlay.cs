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
    
    static VectorPathOverlay()
    {
        AffectsOverlayRender(PathProperty);
    }

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

    private void RenderHandles(Canvas context, IReadOnlyList<VecF> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            pointsHandles[i].Position = new VecD(points[i].X, points[i].Y);
            pointsHandles[i].Draw(context);
        }
    }

    private void AdjustHandles(IReadOnlyList<VecF> points)
    {
        if (pointsHandles.Count != points.Count)
        {
            if (pointsHandles.Count > points.Count)
            {
                pointsHandles.RemoveRange(points.Count, pointsHandles.Count - points.Count);
                Handles.RemoveRange(points.Count, Handles.Count - points.Count);
            }
            else
            {
                for (int i = pointsHandles.Count; i < points.Count; i++)
                {
                    var handle = new AnchorHandle(this);
                    handle.OnDrag += HandleOnOnDrag;
                    handle.OnTap += OnHandleTap;
                    pointsHandles.Add(handle);
                    AddHandle(pointsHandles[i]);
                }
            }
        }
    }

    private void OnHandleTap(Handle handle)
    {
        VectorPath newPath = new VectorPath(Path);
        
        if(IsLastHandle(handle)) return;
        
        if (IsFirstHandle(handle))
        {
            newPath.Close();
        }
        else
        {
            VecD pos = handle.Position;
            newPath.LineTo(new VecF((float)pos.X, (float)pos.Y));
        }

        Path = newPath;
    }

    private bool IsFirstHandle(Handle handle)
    {
        return pointsHandles.IndexOf((AnchorHandle)handle) == 0;
    }
    
    private bool IsLastHandle(Handle handle)
    {
        return pointsHandles.IndexOf((AnchorHandle)handle) == pointsHandles.Count - 1;
    }

    private void HandleOnOnDrag(Handle source, VecD position)
    {
        var handle = (AnchorHandle)source;
        var index = pointsHandles.IndexOf(handle);
        VecF[] updatedPoints = Path.Points.ToArray();
        updatedPoints[index] = new VecF((float)position.X, (float)position.Y);
        VectorPath newPath = new VectorPath();

        newPath.MoveTo(updatedPoints[0]);

        for (var i = 1; i < updatedPoints.Length; i++)
        {
            var point = updatedPoints[i];
            newPath.LineTo(point);
        }

        using var iterator = Path.CreateIterator(false);
        if (iterator.IsCloseContour)
        {
            newPath.Close();
        }

        Path = newPath;
    }
}
