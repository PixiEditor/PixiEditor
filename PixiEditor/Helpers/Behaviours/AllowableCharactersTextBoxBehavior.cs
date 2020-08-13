using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using AvaloniaPlayground.Models;
using System;
using System.Text.RegularExpressions;

namespace PixiEditor.Helpers.Behaviours
{
    public class AllowableCharactersTextBoxBehavior : Behavior<TextBox>
    {
        public static readonly StyledProperty<string> RegularExpressionProperty =
            AvaloniaProperty.Register<AllowableCharactersTextBoxBehavior, string>(nameof(RegularExpression), ".*");

        public static readonly StyledProperty<int> MaxLengthProperty =
            AvaloniaProperty.Register<AllowableCharactersTextBoxBehavior, int>(nameof(MaxLength), int.MinValue);

        public string RegularExpression
        {
            get => (string) GetValue(RegularExpressionProperty);
            set => SetValue(RegularExpressionProperty, value);
        }

        public int MaxLength
        {
            get => (int) GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextInput += OnTextInput;
            AssociatedObject.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && e.KeyModifiers == KeyModifiers.Control)
            {
                string text = Convert.ToString(Application.Current.Clipboard.GetTextAsync());

                if (!IsValid(text, true)) e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            e.Handled = !IsValid(e.Text, false);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextInput -= OnTextInput;
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        private bool IsValid(string newText, bool paste)
        {
            return !ExceedsMaxLength(newText, paste) && Regex.IsMatch(newText, RegularExpression);
        }

        private bool ExceedsMaxLength(string newText, bool paste)
        {
            if (MaxLength == 0) return false;

            return LengthOfModifiedText(newText, paste) > MaxLength;
        }

        private int LengthOfModifiedText(string newText, bool paste)
        {
            var countOfSelectedChars = AssociatedObject.SelectionEnd - AssociatedObject.SelectionStart;
            var caretIndex = AssociatedObject.CaretIndex;
            string text = AssociatedObject.Text;

            if (countOfSelectedChars > 0 || paste)
            {
                text = text.Remove(caretIndex, countOfSelectedChars);
                return text.Length + newText.Length;
            }

            var insert = Keyboard.IsKeyPressed(Key.Insert);

            return insert && caretIndex < text.Length ? text.Length : text.Length + newText.Length;
        }
    }
}