using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public delegate void HandleEvent(Handle source, VecD position);
public abstract class Handle : IHandle
{
    public IBrush HandleBrush { get; set; } = GetBrush("HandleBackgroundBrush");
    public IPen? HandlePen { get; set; }
    public double ZoomboxScale { get; set; } = 1.0;
    public Control Owner { get; set; } = null!;
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

    public Handle(Control owner)
    {
        Owner = owner;
        Position = VecD.Zero;
        Size = Application.Current.TryGetResource("HandleSize", out object size) ? new VecD((double)size) : new VecD(16);

        Owner.PointerPressed += OnPointerPressed;
        Owner.PointerMoved += OnPointerMoved;
        Owner.PointerReleased += OnPointerReleased;
    }

    public abstract void Draw(DrawingContext context);

    public virtual void OnPressed(PointerPressedEventArgs args) { }

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

    public static Geometry GetHandleGeometry(string handleName)
    {
        if (Application.Current.Styles.TryGetResource(handleName, null, out object shape))
        {
            return ((Path)shape).Data.Clone();
        }

        return Geometry.Parse("M 0 0 L 1 0 M 0 0 L 0 1");
    }

    public static HandleGlyph? GetHandleGlyph(string key)
    {
        DrawingGroup? glyph = GetResource<DrawingGroup>(key);
        if (glyph != null)
        {
            glyph.Transform = new MatrixTransform();
            return new HandleGlyph(glyph);
        }

        return null;
    }

    protected static IBrush GetBrush(string key)
    {
        if (Application.Current.Styles.TryGetResource(key, null, out object brush))
        {
            return (IBrush)brush;
        }

        return Brushes.Black;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetMouseButton(Owner) != MouseButton.Left)
        {
            return;
        }

        VecD pos = TransformHelper.ToVecD(e.GetPosition(Owner));
        VecD handlePos = Position;

        if (IsWithinHandle(handlePos, pos, ZoomboxScale))
        {
            e.Handled = true;
            OnPressed(e);
            OnPress?.Invoke(this, pos);
            isPressed = true;
            e.Pointer.Capture(Owner);
        }
    }

    protected virtual void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        VecD pos = TransformHelper.ToVecD(e.GetPosition(Owner));
        VecD handlePos = Position;

        bool isWithinHandle = IsWithinHandle(handlePos, pos, ZoomboxScale);

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

        OnDrag?.Invoke(this, pos);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }

        if (isPressed)
        {
            isPressed = false;
            OnRelease?.Invoke(this);
            e.Pointer.Capture(null);
        }
    }
}
