using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System.Text.RegularExpressions;

namespace PixiEditor.Helpers.Behaviours
{
    internal class TextBoxFocusBehavior : Behavior<TextBox>
    {
        // Using a StyledProperty as the backing store for FillSize.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<bool> FillSizeProperty =
            AvaloniaProperty.Register<TextBoxFocusBehavior, bool>(nameof(FillSize), false);


        private string _oldText; //Value of textbox before editing
        private bool _valueConverted; //This bool is used to avoid double convertion if enter is hitted

        public bool FillSize
        {
            get => (bool) GetValue(FillSizeProperty);
            set => SetValue(FillSizeProperty, value);
        }

        //Converts number to proper format if enter is clicked and moves focus to next object
        private void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            ConvertValue();
            FocusManager.Instance.Focus(null);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObjectGotKeyboardFocus;
            AssociatedObject.PointerPressed += AssociatedObjectGotMouseCapture;
            AssociatedObject.LostFocus += AssociatedObject_LostKeyboardFocus;
            AssociatedObject.KeyUp += AssociatedObject_KeyUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= AssociatedObjectGotKeyboardFocus;
            AssociatedObject.PointerPressed -= AssociatedObjectGotMouseCapture;
            AssociatedObject.LostFocus -= AssociatedObject_LostKeyboardFocus;
            AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
        }

        private void AssociatedObjectGotKeyboardFocus(object sender,
            GotFocusEventArgs e)
        {
            SelectAll();
            if (FillSize)
            {
                _valueConverted = false;
                _oldText = AssociatedObject.Text; //Sets old value when keyboard is focused on object
            }
        }

        private void AssociatedObjectGotMouseCapture(object sender, PointerPressedEventArgs e)
        {
            if (!AssociatedObject.IsFocused)
            {
                AssociatedObject.Focus();
                e.Handled = true;
            }
            SelectAll();
        }

        private void SelectAll()
        {
            AssociatedObject.SelectionStart = 0;
            AssociatedObject.SelectionEnd = AssociatedObject.Text.Length;
        }

        private void AssociatedObject_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            ConvertValue();
        }

        /// <summary>
        ///     Converts number from textbox to format "number px" ex. "15 px"
        /// </summary>
        private void ConvertValue()
        {
            if (_valueConverted || FillSize == false) return;

            if (int.TryParse(Regex.Replace(AssociatedObject.Text, "\\p{L}", ""), out int result) && result > 0)
                AssociatedObject.Text = $"{AssociatedObject.Text} px";
            else //If text in textbox isn't number, set it to old value
                AssociatedObject.Text = _oldText;
            _valueConverted = true;
        }
    }
}