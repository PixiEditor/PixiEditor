using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Models;
using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.UI;

public class AdvisorSlot
{
    public static IAdvisor Current { get; set; }

    public static readonly AttachedProperty<string> AdviceNameProperty =
        AvaloniaProperty.RegisterAttached<AdvisorSlot, Control, string>("AdviceName");

    public static readonly AttachedProperty<ShowDirection> DirectionProperty =
        AvaloniaProperty.RegisterAttached<AdvisorSlot, Control, ShowDirection>("Direction", ShowDirection.Left);

    public static void SetDirection(Control obj, ShowDirection value) => obj.SetValue(DirectionProperty, value);
    public static ShowDirection GetDirection(Control obj) => obj.GetValue(DirectionProperty);

    public static void SetAdviceName(Control obj, string value) => obj.SetValue(AdviceNameProperty, value);
    public static string GetAdviceName(Control obj) => obj.GetValue(AdviceNameProperty);

    static AdvisorSlot()
    {
        AdviceNameProperty.Changed.Subscribe(OnAdviceNameChanged);
        DirectionProperty.Changed.Subscribe(OnDirectionChanged);
    }

    private static void OnAdviceNameChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        if (e.Sender is Control control)
        {
            string newAdviceName = e.NewValue.GetValueOrDefault();
            ShowDirection direction = GetDirection(control);
            Current.SubscribeToAdvisor(newAdviceName, new FlyoutAdviceListener(new AdvisorPopup(control, direction)));
        }
    }

    private static void OnDirectionChanged(AvaloniaPropertyChangedEventArgs<ShowDirection> e)
    {
        if (e.Sender is Control control)
        {
            string adviceName = GetAdviceName(control);
            ShowDirection newDirection = e.NewValue.GetValueOrDefault();
            Current.SubscribeToAdvisor(adviceName, new FlyoutAdviceListener(new AdvisorPopup(control, newDirection)));
        }
    }
}

internal class FlyoutAdviceListener : IAdviceListener
{
    private readonly AdvisorPopup control;

    public FlyoutAdviceListener(AdvisorPopup control)
    {
        this.control = control;
    }

    public void OnAdviceReceived(Advice advice)
    {
        control.Advice = advice;
        control.Show();
    }
}

public enum ShowDirection
{
    Up,
    Down,
    Left,
    Right
}
