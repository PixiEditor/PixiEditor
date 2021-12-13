using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for SizePicker.xaml.
    /// </summary>
    public partial class SizePicker : UserControl
    {
        public static readonly DependencyProperty EditingEnabledProperty =
            DependencyProperty.Register(nameof(EditingEnabled), typeof(bool), typeof(SizePicker), new PropertyMetadata(true));

        public static readonly DependencyProperty ChosenWidthProperty =
            DependencyProperty.Register(nameof(ChosenWidth), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        public static readonly DependencyProperty ChosenHeightProperty =
            DependencyProperty.Register(nameof(ChosenHeight), typeof(int), typeof(SizePicker), new PropertyMetadata(1));

        public static readonly DependencyProperty NextControlProperty =
            DependencyProperty.Register(nameof(NextControl), typeof(FrameworkElement), typeof(SizePicker));

        public SizePicker()
        {
            InitializeComponent();
        }

        public bool EditingEnabled
        {
            get => (bool)GetValue(EditingEnabledProperty);
            set => SetValue(EditingEnabledProperty, value);
        }

        public int ChosenWidth
        {
            get => (int)GetValue(ChosenWidthProperty);
            set => SetValue(ChosenWidthProperty, value);
        }

        public int ChosenHeight
        {
            get => (int)GetValue(ChosenHeightProperty);
            set => SetValue(ChosenHeightProperty, value);
        }

        public FrameworkElement NextControl
        {
            get => (FrameworkElement)GetValue(NextControlProperty);
            set => SetValue(NextControlProperty, value);
        }

        public void FocusWidthPicker()
        {
            WidthPicker.Focus();
        }
    }
}