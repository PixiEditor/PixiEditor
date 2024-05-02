using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Views.Overlays.Handles;
using PixiEditor.AvaloniaUI.Views.Overlays.Transitions;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public abstract class Overlay : Decorator, IOverlay // TODO: Maybe make it not avalonia element
{
    public List<Handle> Handles { get; } = new();

    public virtual OverlayRenderSorting OverlayRenderSorting => OverlayRenderSorting.Foreground;

    public static readonly StyledProperty<double> ZoomScaleProperty =
        AvaloniaProperty.Register<Overlay, double>(nameof(ZoomScale), defaultValue: 1.0);

    public double ZoomScale
    {
        get => GetValue(ZoomScaleProperty);
        set => SetValue(ZoomScaleProperty, value);
    }

    public event Action? RefreshRequested;
    public event PointerEvent? PointerEnteredOverlay;
    public event PointerEvent? PointerExitedOverlay;
    public event PointerEvent? PointerMovedOverlay;
    public event PointerEvent? PointerPressedOverlay;
    public event PointerEvent? PointerReleasedOverlay;

    private readonly Dictionary<AvaloniaProperty, OverlayTransition> activeTransitions = new();

    private DispatcherTimer? transitionTimer;

    public Overlay()
    {
        ZoomScaleProperty.Changed.Subscribe(OnZoomScaleChanged);
    }

    ~Overlay()
    {
        transitionTimer?.Stop();
    }

    public abstract void RenderOverlay(DrawingContext context, RectD canvasBounds);

    public void Refresh()
    {
        RefreshRequested?.Invoke(); // For scene hosted overlays
        InvalidateVisual(); // For elements in visual tree
    }

    public void EnterPointer(OverlayPointerArgs args)
    {
        OnOverlayPointerEntered(args);
        PointerEnteredOverlay?.Invoke(args);
    }

    public void ExitPointer(OverlayPointerArgs args)
    {
        OnOverlayPointerExited(args);
        PointerExitedOverlay?.Invoke(args);
    }

    public void MovePointer(OverlayPointerArgs args)
    {
        OnOverlayPointerMoved(args);
        PointerMovedOverlay?.Invoke(args);
    }

    public void PressPointer(OverlayPointerArgs args)
    {
        OnOverlayPointerPressed(args);
        PointerPressedOverlay?.Invoke(args);
    }

    public void ReleasePointer(OverlayPointerArgs args)
    {
        OnOverlayPointerReleased(args);
        PointerReleasedOverlay?.Invoke(args);
    }

    public virtual bool TestHit(VecD point)
    {
        return Handles.Any(handle => handle.IsWithinHandle(handle.Position, new VecD(point.X, point.Y), ZoomScale));
    }

    public void AddHandle(Handle handle)
    {
        if (Handles.Contains(handle)) return;

        Handles.Add(handle);
    }

    public void ForAllHandles(Action<Handle> action)
    {
        foreach (var handle in Handles)
        {
            action(handle);
        }
    }

    public void ForAllHandles<T>(Action<T> action) where T : Handle
    {
        foreach (var handle in Handles)
        {
            if (handle is T tHandle)
            {
                action(tHandle);
            }
        }
    }

    protected void TransitionTo(AvaloniaProperty property, double durationSeconds, double to, Easing? easing = null)
    {
        object? from = GetValue(property);

        if (from is not double fromDouble)
        {
            throw new InvalidOperationException("Property must be of type double");
        }

        activeTransitions[property] = new OverlayDoubleTransition(durationSeconds, fromDouble, to, easing);
        if (activeTransitions.Count == 1)
        {
            StartTransitions();
        }
    }

    private void StartTransitions()
    {
        transitionTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(16D), DispatcherPriority.Default, (sender, _) =>
        {
            ProgressTransitions(sender as DispatcherTimer);
            Refresh();
        });
    }

    private void ProgressTransitions(DispatcherTimer timer)
    {
        foreach (var transition in activeTransitions)
        {
            transition.Value.Progress += timer.Interval.TotalSeconds / transition.Value.DurationSeconds;
            transition.Value.Progress = Math.Min(transition.Value.Progress, 1);
            SetValue(transition.Key, transition.Value.Evaluate());
        }

        List<KeyValuePair<AvaloniaProperty, OverlayTransition>> transitionsToRemove = activeTransitions
            .Where(t => t.Value.Progress >= 1).ToList();

        foreach (var transition in transitionsToRemove)
        {
            activeTransitions.Remove(transition.Key);
        }

        if (activeTransitions.Count == 0)
        {
            timer.Stop();
        }
    }

    protected virtual void ZoomChanged(double newZoom) { }
    protected virtual void OnOverlayPointerReleased(OverlayPointerArgs args)
    {

    }

    protected virtual void OnOverlayPointerPressed(OverlayPointerArgs args)
    {

    }

    protected virtual void OnOverlayPointerMoved(OverlayPointerArgs args)
    {

    }

    protected virtual void OnOverlayPointerExited(OverlayPointerArgs args)
    {

    }

    protected virtual void OnOverlayPointerEntered(OverlayPointerArgs args)
    {

    }

    private static void OnZoomScaleChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is Overlay overlay)
        {
            overlay.ZoomChanged(e.NewValue.Value);
            foreach (var handle in overlay.Handles)
            {
                handle.ZoomScale = e.NewValue.Value;
            }
        }
    }
}

public enum OverlayRenderSorting
{
    Background,
    Foreground
}
