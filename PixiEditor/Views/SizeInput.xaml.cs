using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for SizeInput.xaml
    /// </summary>
    public partial class SizeInput : UserControl
    {

        private int _loadedSize = -1;
        private int _loadedAspectRatioSize = -1;

        public SizeInput()
        {
            InitializeComponent();
        }



        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(int), typeof(SizeInput), new PropertyMetadata(1));

        public bool PreserveAspectRatio
        {
            get { return (bool)GetValue(PreserveAspectRatioProperty); }
            set { SetValue(PreserveAspectRatioProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PreserveAspectRatio.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreserveAspectRatioProperty =
            DependencyProperty.Register("PreserveAspectRatio", typeof(bool), typeof(SizeInput), new PropertyMetadata(false));


        public int AspectRatioValue
        {
            get { return (int)GetValue(AspectRatioValueProperty); }
            set { SetValue(AspectRatioValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AspectRatioValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AspectRatioValueProperty =
            DependencyProperty.Register("AspectRatioValue", typeof(int), typeof(SizeInput), new PropertyMetadata(1, AspectRatioValChanged));

        private static void AspectRatioValChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SizeInput input = (SizeInput)d;

            if (input.PreserveAspectRatio && input._loadedSize != -1)
            {
                int newVal = (int)e.NewValue;
                float ratio = newVal / Math.Clamp(input._loadedAspectRatioSize, 1f, float.MaxValue);
                input.Size = (int)(input._loadedSize * ratio);
            }
        }

        private void uc_LayoutUpdated(object sender, EventArgs e)
        {
            if(_loadedSize == -1)
            {
                _loadedSize = Size;
                _loadedAspectRatioSize = AspectRatioValue;
            }
        }
    }
}
