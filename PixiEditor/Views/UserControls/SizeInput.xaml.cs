using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SizeInput.xaml.
    /// </summary>
    public partial class SizeInput : UserControl
    {
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(int), typeof(SizeInput), new PropertyMetadata(1, InputSizeChanged));

        public static readonly DependencyProperty PreserveAspectRatioProperty =
            DependencyProperty.Register(
                nameof(PreserveAspectRatio),
                typeof(bool),
                typeof(SizeInput),
                new PropertyMetadata(false));

        public static readonly DependencyProperty AspectRatioValueProperty =
            DependencyProperty.Register(
                nameof(AspectRatioValue),
                typeof(int),
                typeof(SizeInput),
                new PropertyMetadata(1));

        public SizeInput AspectRatioControl
        {
            get { return (SizeInput)GetValue(AspectRatioControlProperty); }
            set { SetValue(AspectRatioControlProperty, value); }
        }

        public static readonly DependencyProperty AspectRatioControlProperty =
            DependencyProperty.Register(nameof(AspectRatioControl), typeof(SizeInput), typeof(SizeInput), new PropertyMetadata(default));

        public static readonly DependencyProperty MaxSizeProperty =
            DependencyProperty.Register(nameof(MaxSize), typeof(int), typeof(SizeInput), new PropertyMetadata(int.MaxValue));

        private int loadedAspectRatioSize = -1;

        private int loadedSize = -1;
        private bool blockUpdate = false;

        public SizeInput()
        {
            InitializeComponent();
        }

        public int Size
        {
            get => (int)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public int MaxSize
        {
            get => (int)GetValue(MaxSizeProperty);
            set => SetValue(MaxSizeProperty, value);
        }

        public bool PreserveAspectRatio
        {
            get => (bool)GetValue(PreserveAspectRatioProperty);
            set => SetValue(PreserveAspectRatioProperty, value);
        }

        public int AspectRatioValue
        {
            get => (int)GetValue(AspectRatioValueProperty);
            set => SetValue(AspectRatioValueProperty, value);
        }

        private static void InputSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue > (int)d.GetValue(MaxSizeProperty))
            {
                int? oldValue = e.OldValue as int?;

                if (oldValue is null)
                {
                    d.SetValue(SizeProperty, 0);
                }
                else
                {
                    d.SetValue(SizeProperty, oldValue.Value);
                }

                return;
            }

            SizeInput input = ((SizeInput)d).AspectRatioControl;
            if (input == null)
            {
                return;
            }

            int newVal = (int)e.NewValue;
            if (input.PreserveAspectRatio && !input.IsFocused && !input.blockUpdate)
            {
                float ratio = newVal / Math.Clamp(input.loadedAspectRatioSize, 1f, float.MaxValue);
                int newSize = (int)(input.loadedSize * ratio);
                input.AspectRatioControl.blockUpdate = true; // Block update is used to prevent infinite feedback loop.
                input.Size = newSize;
            }

            if (input.blockUpdate)
            {
                input.blockUpdate = false;
            }
        }

        private void UserControlLayoutUpdated(object sender, EventArgs e)
        {
            if (loadedSize == -1)
            {
                loadedSize = Size;
                loadedAspectRatioSize = AspectRatioValue;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            textBox.Focus();
        }
    }
}