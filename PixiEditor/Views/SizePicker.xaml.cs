using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SizePicker.xaml
    /// </summary>
    public partial class SizePicker : UserControl
    {
        // Using a DependencyProperty as the backing store for EditingEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditingEnabledProperty =
            DependencyProperty.Register("EditingEnabled", typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

        // Using a DependencyProperty as the backing store for ChosenWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChosenWidthProperty =
            DependencyProperty.Register("ChosenWidth", typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        // Using a DependencyProperty as the backing store for ChoosenHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChoosenHeightProperty =
            DependencyProperty.Register("ChoosenHeight", typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        public SizePicker()
        {
            InitializeComponent();
        }


        public bool EditingEnabled
        {
            get => (bool) GetValue(EditingEnabledProperty);
            set => SetValue(EditingEnabledProperty, value);
        }


        public int ChosenWidth
        {
            get => (int) GetValue(ChosenWidthProperty);
            set => SetValue(ChosenWidthProperty, value);
        }


        public int ChoosenHeight
        {
            get => (int) GetValue(ChoosenHeightProperty);
            set => SetValue(ChoosenHeightProperty, value);
        }
    }
}