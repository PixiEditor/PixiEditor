using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Models.Controllers;

namespace PixiEditor.Helpers.Behaviours;

internal class GlobalShortcutFocusBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.AddHandler(InputElement.GotFocusEvent, AssociatedObject_GotKeyboardFocus);
        AssociatedObject.AddHandler(InputElement.LostFocusEvent, AssociatedObject_LostKeyboardFocus);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.RemoveHandler(InputElement.GotFocusEvent, AssociatedObject_GotKeyboardFocus);
        AssociatedObject.RemoveHandler(InputElement.LostFocusEvent, AssociatedObject_LostKeyboardFocus);
    }

    private void AssociatedObject_LostKeyboardFocus(object sender)
    {
        ShortcutController.UnblockShortcutExecution("GlobalShortcutFocusBehavior");
    }

    private void AssociatedObject_GotKeyboardFocus(object sender)
    {
        ShortcutController.BlockShortcutExecution("GlobalShortcutFocusBehavior");
    }
}
