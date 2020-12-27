using System.Windows;
using System.Windows.Input;
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

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AssociatedObject.Focus();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }
    }
}