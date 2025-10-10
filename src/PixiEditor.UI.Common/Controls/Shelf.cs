using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using PixiEditor.UI.Common.Extensions;
using PixiEditor.UI.Common.Tweening;

namespace PixiEditor.UI.Common.Controls;

[PseudoClasses(":isOpen")]
public class Shelf : ContentControl
{
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Shelf, bool>(
        nameof(IsOpen), true);

    public static readonly RoutedEvent<RoutedEventArgs> OpenedEvent =
        RoutedEvent.Register<Shelf, RoutedEventArgs>(nameof(Opened), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<Shelf, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly StyledProperty<Control?> ControlToCollapseProperty =
        AvaloniaProperty.Register<Shelf, Control?>(
            nameof(ControlToCollapse));

    public Control? ControlToCollapse
    {
        get => GetValue(ControlToCollapseProperty);
        set => SetValue(ControlToCollapseProperty, value);
    }

    private double originalControlHeight;

    private Tweener<double> tween;

    static Shelf()
    {
        IsOpenProperty.Changed.Subscribe(OnIsOpenChanged);
    }

    public Shelf()
    {
        PseudoClasses.Set(":isOpen", true);
    }

    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var shelf = (Shelf)e.Sender;
        shelf.PseudoClasses.Set(":isOpen", (bool)e.NewValue);
        if (e.NewValue is bool isOpen)
        {
            if (isOpen)
            {
                shelf.RaiseEvent(new RoutedEventArgs(OpenedEvent));
                shelf.ExpandControl();
            }
            else
            {
                shelf.RaiseEvent(new RoutedEventArgs(ClosedEvent));
                shelf.CollapseControl();
            }
        }
    }

    private void CollapseControl()
    {
        if (ControlToCollapse == null) ControlToCollapse = this;
        originalControlHeight = ControlToCollapse.Bounds.Height;
        ControlToCollapse.Height = originalControlHeight;

        tween?.Stop();
        tween = Tween.Double(
            Control.HeightProperty,
            ControlToCollapse,
            originalControlHeight - 20,
            300,
            new SineEaseInOut()
        ).Run();
    }

    private void ExpandControl()
    {
        if (ControlToCollapse == null) ControlToCollapse = this;
        tween?.Stop();
        tween = Tween.Double(
            Control.HeightProperty,
            ControlToCollapse,
            originalControlHeight,
            300,
            new SineEaseInOut()
        ).Run();
    }
}
