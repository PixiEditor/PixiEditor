using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PixiEditor.Helpers;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers.Resources;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

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

    public string? ToolTip { get; set; }

    private bool isToolTipVisible;
    private DateTime hoverStarted;
    private const int ToolTipDelayMs = 500;

    private bool isPressed;
    private bool isHovered;
    private bool moved;
    private bool isRendered;

    private static Paint tooltipBackgroundPaint = GetPaint("ThemeBackgroundBrush");
    private static Paint tooltipStrokePaint = GetPaint("ThemeBorderMidBrush", PaintStyle.Stroke);
    private static Paint tooltipForegroundPaint = GetPaint("ThemeForegroundBrush");
    private static Font tooltipFont = Font.CreateDefault();

    public Handle(IOverlay owner)
    {
        Owner = owner;
        Position = VecD.Zero;
        Size = Application.Current.TryGetResource("HandleSize", out object size)
            ? new VecD((double)size)
            : new VecD(16);
    }

    public void Draw(Canvas target)
    {
        isRendered = true;
        OnDraw(target);

        if (isHovered && ToolTip != null)
        {
            if (!isToolTipVisible &&
                (DateTime.UtcNow - hoverStarted).TotalMilliseconds >= ToolTipDelayMs)
            {
                isToolTipVisible = true;
            }

            if (isToolTipVisible)
            {
                DrawToolTip(target);
            }
        }
    }

    protected virtual void DrawToolTip(Canvas target)
    {
        var handlePos = Position;
        double scaleMultiplier = (1.0 / ZoomScale);
        float yOffset = (float)Size.Y * (float)scaleMultiplier;
        var toolTipPos = new VecD(handlePos.X, handlePos.Y - yOffset);

        float radius = 4f * (float)scaleMultiplier;
        tooltipFont.Size = 12 * (float)scaleMultiplier;
        var textSize = new VecD(tooltipFont.MeasureText(ToolTip), 12 * scaleMultiplier);
        var padding = new VecD(6, 3) * scaleMultiplier;
        var backgroundRect = new RectD(toolTipPos.X - padding.X, toolTipPos.Y - textSize.Y - padding.Y / 2f,
            textSize.X + padding.X * 2, textSize.Y + padding.Y * 2);

        target.DrawRoundRect((float)backgroundRect.X, (float)backgroundRect.Y, (float)backgroundRect.Width, (float)backgroundRect.Height, radius, radius, tooltipBackgroundPaint);
        target.DrawRoundRect((float)backgroundRect.X, (float)backgroundRect.Y, (float)backgroundRect.Width, (float)backgroundRect.Height, radius, radius, tooltipStrokePaint);

        target.DrawText(ToolTip, toolTipPos, tooltipFont, tooltipForegroundPaint);
    }

    protected abstract void OnDraw(Canvas target);

    public virtual void OnPressed(OverlayPointerArgs args) { }

    public virtual bool IsWithinHandle(VecD handlePos, VecD pos, double zoomboxScale)
    {
        return TransformHelper.IsWithinHandle(handlePos, pos, zoomboxScale, Size + HitSizeMargin) && isRendered;
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
                try
                {
                    return new PathVectorData(VectorPath.FromSvgPath(path));
                }
                catch (Exception ex)
                {
                    return new PathVectorData(VectorPath.FromSvgPath("M 0 0 L 1 0 M 0 0 L 0 1"));
                }
            }

            if (shape is VectorPathResource resource)
            {
                return resource.ToVectorPathData() ??
                       new PathVectorData(VectorPath.FromSvgPath("M 0 0 L 1 0 M 0 0 L 0 1"));
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
            hoverStarted = DateTime.UtcNow;
            DispatcherTimer.RunOnce(() =>
            {
                Owner.Refresh();
            }, TimeSpan.FromMilliseconds(ToolTipDelayMs));
            isToolTipVisible = false;

            if (Cursor != null)
            {
                Owner.Cursor = Cursor;
            }

            OnHover?.Invoke(this, args);
        }
        else if (isHovered && !isWithinHandle)
        {
            isHovered = false;
            isToolTipVisible = false;
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

    public void ResetIsRendered()
    {
        isRendered = false;
    }

    public void InvokeEnter(OverlayPointerArgs args)
    {
    }

    public void InvokeExit(OverlayPointerArgs args)
    {
        isHovered = false;
        if (isToolTipVisible)
        {
            isToolTipVisible = false;
            Owner.Refresh();
        }
    }
}
