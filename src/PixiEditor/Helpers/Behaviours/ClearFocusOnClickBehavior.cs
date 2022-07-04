using PixiEditor.Models.Controllers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours;

public class ClearFocusOnClickBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        AssociatedObject.LostKeyboardFocus += AssociatedObject_LostKeyboardFocus;
    }

    private void AssociatedObject_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
            
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
    }

    private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        AssociatedObject.Focus();
        ShortcutController.UnblockShortcutExecutionAll();
    }
}