using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
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

    public static readonly StyledProperty<ICommand> AddToUndoCommandProperty =
        AvaloniaProperty.Register<VectorPathOverlay, ICommand>(
            nameof(AddToUndoCommand));

    public ICommand AddToUndoCommand
    {
        get => GetValue(AddToUndoCommandProperty);
        set => SetValue(AddToUndoCommandProperty, value);
    }

    private DashedStroke dashedStroke = new DashedStroke();


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

        AdjustHandles(points, Path.IsClosed);
        RenderHandles(context, points);
    }

    private void RenderHandles(Canvas context, IReadOnlyList<VecF> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            GetHandleAt(i).IsSelected = i == points.Count - 1;

            Handles[i].Position = new VecD(points[i].X, points[i].Y);
            Handles[i].Draw(context);
        }
    }

    private void AdjustHandles(IReadOnlyList<VecF> points, bool isClosed)
    {
        if (Handles.Count != points.Count)
        {
            if (Handles.Count > points.Count)
            {
                Handles.RemoveRange(points.Count, Handles.Count - points.Count);
            }
            else
            {
                for (int i = Handles.Count; i < points.Count; i++)
                {
                    var handle = new AnchorHandle(this);
                    handle.OnPress += OnHandlePress;
                    handle.OnDrag += OnHandleDrag;
                    handle.OnRelease += OnHandleRelease;
                    handle.OnTap += OnHandleTap;
                    AddHandle(handle);
                }
            }
        }
    }

    private void OnHandleTap(Handle handle, OverlayPointerArgs args)
    {
        if (Path.IsClosed)
        {
            return;    
        }
        
        VectorPath newPath = new VectorPath(Path);
        if (args.Modifiers.HasFlag(KeyModifiers.Control)) return;

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
        return Handles.IndexOf(handle) == 0;
    }

    private void OnHandleDrag(Handle source, OverlayPointerArgs args)
    {
        var handle = (AnchorHandle)source;

        var index = Handles.IndexOf(handle);
        VectorPath newPath = new VectorPath();

        bool pointHandled = false;
        int i = 0;
        foreach (var data in Path)
        {
            VecF point;
            switch (data.verb)
            {
                case PathVerb.Move:
                    point = data.points[0];
                    if (i == index)
                    {
                        point = (VecF)args.Point;
                    }

                    newPath.MoveTo(point);
                    i++;
                    break;
                case PathVerb.Line:
                    point = data.points[1];
                    if (i == index)
                    {
                        point = (VecF)args.Point;
                    }

                    newPath.LineTo(point);
                    i++;
                    break;
                /*case PathVerb.Quad:
                    newPath.QuadTo(data.points[0], point);
                    break;
                case PathVerb.Conic:
                    newPath.ConicTo(data.points[0], point, data.points[2].X);
                    break;
                case PathVerb.Cubic:
                    newPath.CubicTo(data.points[0], data.points[1], point);
                    break;*/
                case PathVerb.Close:
                    newPath.Close();
                    i++;
                    break;
                case PathVerb.Done:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Path = newPath;
    }

    private void OnHandlePress(Handle source, OverlayPointerArgs args)
    {
    }

    private AnchorHandle GetHandleAt(int index)
    {
        if (index < 0 || index >= Handles.Count)
        {
            return null;
        }

        if (Handles[index] is AnchorHandle handle)
        {
            return handle;
        }

        return null;
    }

    private void OnHandleRelease(Handle source, OverlayPointerArgs args)
    {
        AddToUndoCommand.Execute(Path);

        Refresh();
    }
}
