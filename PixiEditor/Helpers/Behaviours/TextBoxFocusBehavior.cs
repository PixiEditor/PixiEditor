using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PixiEditor.Helpers.Behaviours
{
    internal class TextBoxFocusBehavior : Behavior<TextBox>
    {
        // Using a DependencyProperty as the backing store for FillSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectOnFocusProperty =
            DependencyProperty.Register(
                nameof(SelectOnFocus),
                typeof(bool),
                typeof(TextBoxFocusBehavior),
                new PropertyMetadata(true));

        public static readonly DependencyProperty NextControlProperty =
            DependencyProperty.Register(nameof(NextControl), typeof(FrameworkElement), typeof(TextBoxFocusBehavior));

        public FrameworkElement NextControl
        {
            get => (FrameworkElement)GetValue(NextControlProperty);
            set => SetValue(NextControlProperty, value);
        }

        public bool SelectOnFocus
        {
            get => (bool)GetValue(SelectOnFocusProperty);
            set => SetValue(SelectOnFocusProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture += AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture -= AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        // Converts number to proper format if enter is clicked and moves focus to next object
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            RemoveFocus();
        }

        private void RemoveFocus()
        {
            DependencyObject scope = FocusManager.GetFocusScope(AssociatedObject);

            if (NextControl != null)
            {
                FocusManager.SetFocusedElement(scope, NextControl);
                return;
            }

            FrameworkElement parent = (FrameworkElement)AssociatedObject.Parent;

            while (parent != null && parent is IInputElement element && !element.Focusable)
            {
                parent = (FrameworkElement)parent.Parent;
            }

            FocusManager.SetFocusedElement(scope, parent);
        }

        private void AssociatedObjectGotKeyboardFocus(
            object sender,
            KeyboardFocusChangedEventArgs e)
        {
            if (SelectOnFocus)
                AssociatedObject.SelectAll();
        }

        private void AssociatedObjectGotMouseCapture(
            object sender,
            MouseEventArgs e)
        {
            if (SelectOnFocus)
                AssociatedObject.SelectAll();
        }

        private void AssociatedObjectPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                AssociatedObject.Focus();
                e.Handled = true;
            }
        }
    }
}
