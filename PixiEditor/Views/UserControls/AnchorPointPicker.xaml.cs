using PixiEditor.Models.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for AnchorPointPicker.xaml
    /// </summary>
    public partial class AnchorPointPicker : UserControl
    {
        public static readonly DependencyProperty AnchorPointProperty =
            DependencyProperty.Register("AnchorPoint", typeof(AnchorPoint), typeof(AnchorPointPicker),
                new PropertyMetadata());


        private ToggleButton _selectedToggleButton;

        public AnchorPointPicker()
        {
            InitializeComponent();
        }

        public AnchorPoint AnchorPoint
        {
            get => (AnchorPoint)GetValue(AnchorPointProperty);
            set => SetValue(AnchorPointProperty, value);
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            AnchorPoint = (AnchorPoint)(1 << (Grid.GetRow(btn) + 3)) | (AnchorPoint)(1 << Grid.GetColumn(btn));
            if (_selectedToggleButton != null) _selectedToggleButton.IsChecked = false;
            _selectedToggleButton = btn;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked.Value)
                e.Handled = true;
        }
    }
}
