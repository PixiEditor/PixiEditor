using PixiEditor.Models.Controllers.Shortcuts;
using System.Windows;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours
{
    public class ClearFocusOnClickBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            base.OnAttached();
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
}
