using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.Handles;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public class Overlay : Decorator
{
    public List<Handle> Handles { get; } = new();

    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<Overlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public double ZoomboxScale
    {
        get => GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public Overlay()
    {
        ZoomboxScaleProperty.Changed.Subscribe(OnZoomboxScaleChanged);
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

    private static void OnZoomboxScaleChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is Overlay overlay)
        {
            foreach (var handle in overlay.Handles)
            {
                handle.ZoomboxScale = e.NewValue.Value;
            }
        }
    }
}
