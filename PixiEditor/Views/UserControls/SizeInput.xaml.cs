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
        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(int), typeof(SizeInput), new PropertyMetadata(1, InputSizeChanged));

        // Using a DependencyProperty as the backing store for PreserveAspectRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreserveAspectRatioProperty =
            DependencyProperty.Register(
                "PreserveAspectRatio",
                typeof(bool),
                typeof(SizeInput),
                new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for AspectRatioValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioValueProperty =
            DependencyProperty.Register(
                "AspectRatioValue",
                typeof(int),
                typeof(SizeInput),
                new PropertyMetadata(1));

        public SizeInput AspectRatioControl
        {
            get { return (SizeInput)GetValue(AspectRatioControlProperty); }
            set { SetValue(AspectRatioControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AspectRatioControl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioControlProperty =
            DependencyProperty.Register("AspectRatioControl", typeof(SizeInput), typeof(SizeInput), new PropertyMetadata(default));

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
    }
}