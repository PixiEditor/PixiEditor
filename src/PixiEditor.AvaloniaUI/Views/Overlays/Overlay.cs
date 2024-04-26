using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Views.Overlays.Handles;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public abstract class Overlay : Decorator // TODO: Maybe make it not avalonia element
{
    public List<Handle> Handles { get; } = new();

    public static readonly StyledProperty<double> ZoomScaleProperty =
        AvaloniaProperty.Register<Overlay, double>(nameof(ZoomScale), defaultValue: 1.0);

    public double ZoomScale
    {
        get => GetValue(ZoomScaleProperty);
        set => SetValue(ZoomScaleProperty, value);
    }

    public event Action? RefreshRequested;

    public Overlay()
    {
        ZoomScaleProperty.Changed.Subscribe(OnZoomboxScaleChanged);
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

    protected virtual void ZoomChanged(double newZoom) { }

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is Overlay overlay)
        {
            overlay.ZoomChanged(e.NewValue.Value);
            foreach (var handle in overlay.Handles)
            {
                handle.ZoomboxScale = e.NewValue.Value;
            }
        }
    }

    public virtual bool TestHit(VecD point)
    {
        return Handles.Any(handle => handle.HandleRect.ContainsInclusive(new VecD(point.X, point.Y)));
    }

    public void Refresh()
    {
        RefreshRequested?.Invoke(); // For scene hosted overlays
        InvalidateVisual(); // For elements in visual tree
    }

    public virtual void PointerEnteredOverlay(OverlayPointerArgs args)
    {
    }

    public virtual void PointerExitedOverlay(OverlayPointerArgs args)
    {

    }

    public virtual void PointerMovedOverlay(OverlayPointerArgs args)
    {

    }

    public virtual void PointerPressedOverlay(OverlayPointerArgs args)
    {

    }

    public virtual void PointerReleasedOverlay(OverlayPointerArgs args)
    {

    }
}
