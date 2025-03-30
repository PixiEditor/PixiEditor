using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.Helpers;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Helpers.Resources;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Path = Avalonia.Controls.Shapes.Path;

namespace PixiEditor.Views.Overlays.Handles;

public delegate void HandleEvent(Handle source, OverlayPointerArgs args);

public abstract class Handle : IHandle
{
    public string Name { get; set; }
    public Paint? FillPaint { get; set; } = GetPaint("HandleBackgroundBrush");
    public Paint? StrokePaint { get; set; } = GetPaint("HandleBrush", PaintStyle.Stroke);
    public double ZoomScale { get; set; } = 1.0;
    public IOverlay Owner { get; set; } = null!;
    public VecD Position { get; set; }
    public VecD Size { get; set; }
    public RectD HandleRect => new(Position, Size);
    public bool HitTestVisible { get; set; } = true;
    public bool IsHovered => isHovered;

    public virtual VecD HitSizeMargin { get; set; } = VecD.Zero;

    public event HandleEvent OnPress;
    public event HandleEvent OnDrag;
    public event HandleEvent OnRelease;
    public event HandleEvent OnHover;
    public event HandleEvent OnExit;
    public event HandleEvent OnTap;
    public Cursor? Cursor { get; set; }

    private bool isPressed;
    private bool isHovered;
    private bool moved;

    public Handle(IOverlay owner)
    {
        Owner = owner;
        Position = VecD.Zero;
        Size = Application.Current.TryGetResource("HandleSize", out object size)
            ? new VecD((double)size)
            : new VecD(16);
    }

    public abstract void Draw(Canvas target);

    public virtual void OnPressed(OverlayPointerArgs args) { }

    public virtual bool IsWithinHandle(VecD handlePos, VecD pos, double zoomboxScale)
    {
        return TransformHelper.IsWithinHandle(handlePos, pos, zoomboxScale, Size + HitSizeMargin);
    }

    public static T? GetResource<T>(string key)
    {
        return ResourceLoader.GetResource<T>(key);
    }

    public static PathVectorData GetHandleGeometry(string handleName)
    {
        if (Application.Current.Styles.TryGetResource(handleName, null, out object shape))
        {
            if (shape is string path)
            {
                return new PathVectorData(VectorPath.FromSvgPath(path));
            }

            if (shape is VectorPathResource resource)
            {
                return resource.ToVectorPathData();
            }
        }

        return new PathVectorData(VectorPath.FromSvgPath("M 0 0 L 1 0 M 0 0 L 0 1"));
    }

    protected static Paint? GetPaint(string key, PaintStyle style = PaintStyle.Fill)
    {
        return ResourceLoader.GetPaint(key, style);
    }

    public void InvokePress(OverlayPointerArgs args)
    {
        OnPointerPressed(args);
    }

    public void InvokeMove(OverlayPointerArgs args)
    {
        OnPointerMoved(args);
    }

    public void InvokeRelease(OverlayPointerArgs args)
    {
        OnPointerReleased(args);
    }

    private void OnPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
        {
            return;
        }

        if (args.Handled)
        {
            return;
        }

        VecD handlePos = Position;

        if (IsWithinHandle(handlePos, args.Point, ZoomScale) && HitTestVisible)
        {
            args.Handled = true;
            OnPressed(args);
            moved = false;
            OnPress?.Invoke(this, args);
            isPressed = true;
            args.Pointer.Capture(Owner);
        }
    }

    protected virtual void OnPointerMoved(OverlayPointerArgs args)
    {
        VecD handlePos = Position;

        if (args.Handled || !HitTestVisible)
        {
            return;
        }

        bool isWithinHandle = IsWithinHandle(handlePos, args.Point, ZoomScale);

        if (!isHovered && isWithinHandle)
        {
            isHovered = true;
            if (Cursor != null)
            {
                Owner.Cursor = Cursor;
            }

            OnHover?.Invoke(this, args);
        }
        else if (isHovered && !isWithinHandle)
        {
            isHovered = false;
            Owner.Cursor = null;
            OnExit?.Invoke(this, args);
        }

        if (!isPressed)
        {
            return;
        }

        OnDrag?.Invoke(this, args);
        args.Handled = true;
        moved = true;
    }

    private void OnPointerReleased(OverlayPointerArgs args)
    {
        if (args.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        if (args.Handled || !HitTestVisible)
        {
            isPressed = false;
            return;
        }

        if (isPressed)
        {
            isPressed = false;
            if (!moved)
            {
                OnTap?.Invoke(this, args);
            }

            OnRelease?.Invoke(this, args);
            args.Pointer.Capture(null);
            args.Handled = true;
        }
    }
}
