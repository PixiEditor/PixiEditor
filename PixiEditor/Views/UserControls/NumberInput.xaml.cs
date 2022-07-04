using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for NumerInput.xaml.
    /// </summary>
    public partial class NumberInput : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(float),
                typeof(NumberInput),
                new PropertyMetadata(0f, OnValueChanged));

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(
                nameof(Min),
                typeof(float),
                typeof(NumberInput),
                new PropertyMetadata(float.NegativeInfinity));

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(
                nameof(Max),
                typeof(float),
                typeof(NumberInput),
                new PropertyMetadata(float.PositiveInfinity));

        private readonly Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$", RegexOptions.Compiled);

        public int Decimals
        {
            get { return (int)GetValue(DecimalsProperty); }
            set { SetValue(DecimalsProperty, value); }
        }

        public static readonly DependencyProperty DecimalsProperty =
            DependencyProperty.Register(nameof(Decimals), typeof(int), typeof(NumberInput), new PropertyMetadata(2));

        public Action OnScrollAction
        {
            get { return (Action)GetValue(OnScrollActionProperty); }
            set { SetValue(OnScrollActionProperty, value); }
        }

        public static readonly DependencyProperty OnScrollActionProperty =
            DependencyProperty.Register(nameof(OnScrollAction), typeof(Action), typeof(NumberInput), new PropertyMetadata(null));

        public NumberInput()
        {
            InitializeComponent();
        }

        public float Value
        {
            get => (float)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public float Min
        {
            get => (float)GetValue(MinProperty);
            set => SetValue(MinProperty, value);
        }

        public float Max
        {
            get => (float)GetValue(MaxProperty);
            set => SetValue(MaxProperty, value);
        }

        public static readonly DependencyProperty FocusNextProperty = DependencyProperty.Register(nameof(FocusNext), typeof(bool), typeof(NumberInput), new PropertyMetadata(false));

        public bool FocusNext
        {
            get { return (bool)GetValue(FocusNextProperty); }
            set { SetValue(FocusNextProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberInput input = (NumberInput)d;
            input.Value = (float)Math.Round(Math.Clamp((float)e.NewValue, input.Min, input.Max), input.Decimals);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int step = e.Delta / 100;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                float multiplier = (Max - Min) * 0.1f;
                Value += step * multiplier;
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Value += step / 2f;
            }
            else
            {
                Value += step;
            }

            OnScrollAction?.Invoke();
        }
    }
}
