using System.Windows;
using System.Windows.Interactivity;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Helpers.Behaviours;

internal class GlobalShortcutFocusBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.GotKeyboardFocus += AssociatedObject_GotKeyboardFocus;
        AssociatedObject.LostKeyboardFocus += AssociatedObject_LostKeyboardFocus;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.GotKeyboardFocus -= AssociatedObject_GotKeyboardFocus;
        AssociatedObject.LostKeyboardFocus -= AssociatedObject_LostKeyboardFocus;
    }

    private void AssociatedObject_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        ShortcutController.UnblockShortcutExecution("GlobalShortcutFocusBehavior");
    }

    private void AssociatedObject_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        ShortcutController.BlockShortcutExection("GlobalShortcutFocusBehavior");
    }
}
