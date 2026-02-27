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
            IAdvisor.Current.SubscribeToAdvisor(newAdviceName, new FlyoutAdviceListener(new AdvisorPopup(control)));
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
