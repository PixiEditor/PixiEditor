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
        public static readonly DependencyProperty FillSizeProperty =
            DependencyProperty.Register(
                "FillSize",
                typeof(bool),
                typeof(TextBoxFocusBehavior),
                new PropertyMetadata(false));

        private string oldText; // Value of textbox before editing
        private bool valueConverted; // This bool is used to avoid double convertion if enter is hitted

        public bool FillSize
        {
            get => (bool)GetValue(FillSizeProperty);
            set => SetValue(FillSizeProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture += AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.LostKeyboardFocus += AssociatedObject_LostKeyboardFocus;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.GotMouseCapture -= AssociatedObjectGotMouseCapture;
            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.LostKeyboardFocus -= AssociatedObject_LostKeyboardFocus;
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        // Converts number to proper format if enter is clicked and moves focus to next object
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            ConvertValue();
            AssociatedObject.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
        }

        private void AssociatedObjectGotKeyboardFocus(
            object sender,
            KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
            if (FillSize)
            {
                valueConverted = false;
                oldText = AssociatedObject.Text; // Sets old value when keyboard is focused on object
            }
        }

        private void AssociatedObjectGotMouseCapture(
            object sender,
            MouseEventArgs e)
        {
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

        private void AssociatedObject_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ConvertValue();
        }

        /// <summary>
        ///     Converts number from textbox to format "number px" ex. "15 px".
        /// </summary>
        private void ConvertValue()
        {
            if (valueConverted || FillSize == false || AssociatedObject.Text == oldText)
            {
                return;
            }

            if (int.TryParse(Regex.Replace(AssociatedObject.Text, "\\p{L}", string.Empty).Trim(), out int result) && result > 0)
            {
                AssociatedObject.Text = $"{result} px";
            }

            // If text in textbox isn't number, set it to old value
            else
            {
                AssociatedObject.Text = oldText;
            }

            valueConverted = true;
        }
    }
}