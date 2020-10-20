using System;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SizeInput.xaml
    /// </summary>
    public partial class SizeInput : UserControl
    {
        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(int), typeof(SizeInput), new PropertyMetadata(1));

        // Using a DependencyProperty as the backing store for PreserveAspectRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreserveAspectRatioProperty =
            DependencyProperty.Register("PreserveAspectRatio", typeof(bool), typeof(SizeInput),
                new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for AspectRatioValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioValueProperty =
            DependencyProperty.Register("AspectRatioValue", typeof(int), typeof(SizeInput),
                new PropertyMetadata(1, AspectRatioValChanged));

        private int loadedAspectRatioSize = -1;

        private int loadedSize = -1;

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

        private static void AspectRatioValChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SizeInput input = (SizeInput)d;

            if (input.PreserveAspectRatio && input.loadedSize != -1)
            {
                int newVal = (int)e.NewValue;
                float ratio = newVal / Math.Clamp(input.loadedAspectRatioSize, 1f, float.MaxValue);
                input.Size = (int)(input.loadedSize * ratio);
            }
        }

        private void uc_LayoutUpdated(object sender, EventArgs e)
        {
            if (loadedSize == -1)
            {
                loadedSize = Size;
                loadedAspectRatioSize = AspectRatioValue;
            }
        }
    }
}