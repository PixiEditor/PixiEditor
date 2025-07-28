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
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.Handles;
using PixiEditor.Views.Overlays.Transitions;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays;

enum HandleEventType
{
    PointerEnteredOverlay,
    PointerExitedOverlay,
    PointerMovedOverlay,
    PointerPressedOverlay,
    PointerReleasedOverlay
}

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
    public event Action? FocusRequested;
    public event Action? RefreshCursorRequested;
    public event PointerEvent? PointerEnteredOverlay;
    public event PointerEvent? PointerExitedOverlay;
    public event PointerEvent? PointerMovedOverlay;
    public event PointerEvent? PointerPressedOverlay;
    public event PointerEvent? PointerReleasedOverlay;
    public event KeyEvent? KeyPressedOverlay;
    public event KeyEvent? KeyReleasedOverlay;

    public Handle? CapturedHandle { get; set; } = null!;
    public VecD PointerPosition { get; internal set; }

    public static readonly StyledProperty<bool> SuppressEventsProperty = AvaloniaProperty.Register<Overlay, bool>(
        nameof(SuppressEvents));

    public bool SuppressEvents
    {
        get => GetValue(SuppressEventsProperty);
        set
        {
            SetValue(SuppressEventsProperty, value);
        }
    }

    private readonly Dictionary<AvaloniaProperty, OverlayTransition> activeTransitions = new();

    private DispatcherTimer? transitionTimer;

    public Overlay()
    {
        ZoomScaleProperty.Changed.Subscribe(OnZoomScaleChanged);
        IsVisibleProperty.Changed.Subscribe(OnIsVisibleChanged);
    }

    ~Overlay()
    {
        transitionTimer?.Stop();
    }

    public virtual bool CanRender() => true;

    public abstract void RenderOverlay(Canvas context, RectD canvasBounds);

    public void Refresh()
    {
        RefreshRequested?.Invoke(); // For scene hosted overlays
        InvalidateVisual(); // For elements in visual tree
    }

    public void FocusOverlay()
    {
        FocusRequested?.Invoke();
    }

    public void ForceRefreshCursor()
    {
        RefreshCursorRequested?.Invoke();
    }

    public void CaptureHandle(Handle handle)
    {
        CapturedHandle = handle;
    }

    public void EnterPointer(OverlayPointerArgs args)
    {
        if(SuppressEvents) return;
        OnOverlayPointerEntered(args);
        if (args.Handled) return;
        InvokeHandleEvent(HandleEventType.PointerEnteredOverlay, args);
        if (args.Handled) return;
        PointerEnteredOverlay?.Invoke(args);
    }

    public void ExitPointer(OverlayPointerArgs args)
    {
        if(SuppressEvents) return;
        OnOverlayPointerExited(args);
        if (args.Handled) return;
        InvokeHandleEvent(HandleEventType.PointerExitedOverlay, args);
        if (args.Handled) return;
        PointerExitedOverlay?.Invoke(args);
    }

    public void MovePointer(OverlayPointerArgs args)
    {
        if(SuppressEvents) return;
        InvokeHandleEvent(HandleEventType.PointerMovedOverlay, args);
        if (args.Handled) return;
        OnOverlayPointerMoved(args);
        if (args.Handled) return;
        PointerMovedOverlay?.Invoke(args);
    }

    public void FocusChanged(bool focused)
    {
        if (focused)
        {
            OnOverlayGotFocus();
        }
        else
        {
            OnOverlayLostFocus();
        }
    }

    public void PressPointer(OverlayPointerArgs args)
    {
        if(SuppressEvents) return;
        InvokeHandleEvent(HandleEventType.PointerPressedOverlay, args);
        if (args.Handled) return;
        OnOverlayPointerPressed(args);
        if (args.Handled) return;
        PointerPressedOverlay?.Invoke(args);
    }

    public void ReleasePointer(OverlayPointerArgs args)
    {
        if(SuppressEvents) return;
        InvokeHandleEvent(HandleEventType.PointerReleasedOverlay, args);
        if (args.Handled)
        {
            CaptureHandle(null);
            return;
        }
        OnOverlayPointerReleased(args);
        if (args.Handled)
        {
            CaptureHandle(null);
            return;
        }

        PointerReleasedOverlay?.Invoke(args);
    }
    
    public void KeyPressed(KeyEventArgs args)
    {
        if(SuppressEvents) return;
        if (args.Handled) return;
        OnKeyPressed(args);
        if (args.Handled) return;
        KeyPressedOverlay?.Invoke(args.Key, args.KeyModifiers);
    }

    public void KeyReleased(KeyEventArgs keyEventArgs)
    {
        if(SuppressEvents) return;
        if (keyEventArgs.Handled) return;
        OnKeyReleased(keyEventArgs);
        if (keyEventArgs.Handled) return;
        KeyReleasedOverlay?.Invoke(keyEventArgs.Key, keyEventArgs.KeyModifiers);
    }

    public virtual bool TestHit(VecD point)
    {
        return !SuppressEvents && Handles.Any(handle => handle.IsWithinHandle(handle.Position, new VecD(point.X, point.Y), ZoomScale));
    }

    public void AddHandle(Handle handle)
    {
        if (Handles.Contains(handle)) return;

        Handles.Add(handle);
        handle.ZoomScale = ZoomScale;
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

    private void InvokeHandleEvent(HandleEventType eventName, OverlayPointerArgs args)
    {
        if (CapturedHandle != null)
        {
            InvokeHandleEvent(CapturedHandle, args, eventName);
        }
        else
        {
            var reversedHandles = Handles.Reverse<Handle>();
            foreach (var handle in reversedHandles)
            {
                InvokeHandleEvent(handle, args, eventName);
            }
        }
    }

    private void InvokeHandleEvent(Handle handle, OverlayPointerArgs args, HandleEventType pointerEvent)
    {
        if (pointerEvent == null) return;

        if (pointerEvent == HandleEventType.PointerMovedOverlay)
        {
            handle.InvokeMove(args);
        }
        else if (pointerEvent == HandleEventType.PointerPressedOverlay)
        {
            handle.InvokePress(args);
        }
        else if (pointerEvent == HandleEventType.PointerReleasedOverlay)
        {
            handle.InvokeRelease(args);
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
    
    protected virtual void OnKeyPressed(KeyEventArgs args)
    {
    }
    
    protected virtual void OnKeyReleased(KeyEventArgs args)
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

    protected virtual void OnOverlayLostFocus()
    {
    }

    protected virtual void OnOverlayGotFocus()
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

    private void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.NewValue.Value)
        {
            Refresh();
        }
        else
        {
            Cursor = null;
            RefreshCursorRequested?.Invoke();
        }
    }

    protected static void AffectsOverlayRender(params AvaloniaProperty[] properties)
    {
        foreach (var property in properties)
        {
            property.Changed.Subscribe((args) =>
            {
                if (args.Sender is Overlay overlay)
                {
                    overlay.Refresh();
                }
            });
        }
    }
}

public enum OverlayRenderSorting
{
    Background,
    Foreground
}
