using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.Helpers;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Surfaces.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Path = Avalonia.Controls.Shapes.Path;

namespace PixiEditor.Views.Overlays.Handles;

public delegate void HandleEvent(Handle source, VecD position);
public abstract class Handle : IHandle
{
    public Paint? FillPaint { get; set; } = GetPaint("HandleBackgroundBrush");
    public Paint? StrokePaint { get; set; } = GetPaint("HandleStrokeBrush", PaintStyle.Stroke);
    public double ZoomScale { get; set; } = 1.0;
    public IOverlay Owner { get; set; } = null!;
    public VecD Position { get; set; }
    public VecD Size { get; set; }
    public RectD HandleRect => new(Position, Size);

    public event HandleEvent OnPress;
    public event HandleEvent OnDrag;
    public event Action<Handle> OnRelease;
    public event Action<Handle> OnHover;
    public event Action<Handle> OnExit;
    public Cursor? Cursor { get; set; }

    private bool isPressed;
    private bool isHovered;

    public Handle(IOverlay owner)
    {
        Owner = owner;
        Position = VecD.Zero;
        Size = Application.Current.TryGetResource("HandleSize", out object size) ? new VecD((double)size) : new VecD(16);

        Owner.PointerPressedOverlay += OnPointerPressed;
        Owner.PointerMovedOverlay += OnPointerMoved;
        Owner.PointerReleasedOverlay += OnPointerReleased;
    }

    public abstract void Draw(Canvas target);

    public virtual void OnPressed(OverlayPointerArgs args) { }

    public virtual bool IsWithinHandle(VecD handlePos, VecD pos, double zoomboxScale)
    {
        return TransformHelper.IsWithinHandle(handlePos, pos, zoomboxScale, Size);
    }

    public static T? GetResource<T>(string key)
    {
        if (Application.Current.Styles.TryGetResource(key, null, out object resource))
        {
            return (T)resource;
        }

        return default!;
    }

    public static VectorPath GetHandleGeometry(string handleName)
    {
        if (Application.Current.Styles.TryGetResource(handleName, null, out object shape))
        {
            if (shape is string path)
            {
                return VectorPath.FromSvgPath(path);
            }
        }

        return VectorPath.FromSvgPath("M 0 0 L 1 0 M 0 0 L 0 1");
    }

    protected static Paint? GetPaint(string key, PaintStyle style = PaintStyle.Fill)
    {
        if (Application.Current.Styles.TryGetResource(key, null, out object paint))
        {
            if (paint is SolidColorBrush solidColorBrush)
            {
                return new Paint() { Color = solidColorBrush.Color.ToColor(), Style = style, IsAntiAliased = true };
            }
            
            throw new InvalidOperationException("Invalid paint style");
        }

        return null;
    }

    private void OnPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
        {
            return;
        }

        VecD handlePos = Position;

        if (IsWithinHandle(handlePos, args.Point, ZoomScale))
        {
            args.Handled = true;
            OnPressed(args);
            OnPress?.Invoke(this, args.Point);
            isPressed = true;
            args.Pointer.Capture(Owner);
        }
    }

    protected virtual void OnPointerMoved(OverlayPointerArgs args)
    {
        VecD handlePos = Position;

        bool isWithinHandle = IsWithinHandle(handlePos, args.Point, ZoomScale);

        if (!isHovered && isWithinHandle)
        {
            isHovered = true;
            if (Cursor != null)
            {
                Owner.Cursor = Cursor;
            }

            OnHover?.Invoke(this);
        }
        else if (isHovered && !isWithinHandle)
        {
            isHovered = false;
            Owner.Cursor = null;
            OnExit?.Invoke(this);
        }

        if (!isPressed)
        {
            return;
        }

        OnDrag?.Invoke(this, args.Point);
    }

    private void OnPointerReleased(OverlayPointerArgs args)
    {
        if (args.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        if (isPressed)
        {
            isPressed = false;
            OnRelease?.Invoke(this);
            args.Pointer.Capture(null);
        }
    }
}
