using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace PixiEditor.Helpers.Behaviours
{
    internal class HintTextBehavior : Behavior<TextBox>
    {
        // Using a DependencyProperty as the backing store for Hint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(HintTextBehavior),
                new PropertyMetadata(string.Empty));

        private Brush _textColor;

        public string Hint
        {
            get => (string) GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            _textColor = AssociatedObject.Foreground;
            SetHint(true);
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AssociatedObject.Text)) SetHint(true);
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.Text == Hint) SetHint(false);
        }

        private void SetHint(bool active)
        {
            if (active)
            {
                AssociatedObject.Foreground = (SolidColorBrush) new BrushConverter().ConvertFromString("#7B7B7B");
                AssociatedObject.Text = Hint;
            }
            else
            {
                AssociatedObject.Text = string.Empty;
                AssociatedObject.Foreground = _textColor;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
        }
    }
}