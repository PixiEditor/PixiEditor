using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using PixiEditor.Models;
using PixiEditor.Models.AdvisorSystem;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Helpers.UI;

public class AdvisorSlot
{
    public static readonly AttachedProperty<string> AdviceNameProperty =
        AvaloniaProperty.RegisterAttached<AdvisorSlot, Control, string>("AdviceName");
    public static void SetAdviceName(Control obj, string value) => obj.SetValue(AdviceNameProperty, value);
    public static string GetAdviceName(Control obj) => obj.GetValue(AdviceNameProperty);

    static AdvisorSlot()
    {
        AdviceNameProperty.Changed.Subscribe(OnAdviceNameChanged);
    }

    private static void OnAdviceNameChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        if (e.Sender is Control control)
        {
            string newAdviceName = e.NewValue.GetValueOrDefault();
            FlyoutBase.SetAttachedFlyout(control, new AdvisorFlyout(control));
            IAdvisor.Current.SubscribeToAdvisor(newAdviceName, new FlyoutAdviceListener(control));
        }
    }
}

internal class FlyoutAdviceListener : IAdviceListener
{
    private readonly Control control;

    public FlyoutAdviceListener(Control control)
    {
        this.control = control;
    }

    public void OnAdviceReceived(Advice advice)
    {
        if (FlyoutBase.GetAttachedFlyout(control) is AdvisorFlyout flyout)
        {
            flyout.Advice = advice.Content;
            flyout.ShowAt(control);
        }
    }
}
