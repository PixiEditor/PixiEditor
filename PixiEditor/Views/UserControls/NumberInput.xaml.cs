using PixiEditor.Models.Controllers.Shortcuts;
using System;
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
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(float),
                typeof(NumberInput),
                new PropertyMetadata(0f, OnValueChanged));

        // Using a DependencyProperty as the backing store for Min.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(
                "Min",
                typeof(float),
                typeof(NumberInput),
                new PropertyMetadata(float.NegativeInfinity));

        // Using a DependencyProperty as the backing store for Max.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(
                "Max",
                typeof(float), 
                typeof(NumberInput),
                new PropertyMetadata(float.PositiveInfinity));

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

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberInput input = (NumberInput) d;
            input.Value = Math.Clamp((float) e.NewValue, input.Min, input.Max);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));
        }
    }
}