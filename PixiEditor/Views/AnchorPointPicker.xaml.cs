using PixiEditor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for AnchorPointPicker.xaml
    /// </summary>
    public partial class AnchorPointPicker : UserControl
    {

        public AnchorPoint AnchorPoint
        {
            get { return (AnchorPoint)GetValue(AnchorPointProperty); }
            set { SetValue(AnchorPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AnchorPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnchorPointProperty =
            DependencyProperty.Register("AnchorPoint", typeof(AnchorPoint), typeof(AnchorPointPicker), new PropertyMetadata());


        private ToggleButton _selectedToggleButton;
        public AnchorPointPicker()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            AnchorPoint = (AnchorPoint)(1 << Grid.GetRow(btn) + 3) | (AnchorPoint)(1 << Grid.GetColumn(btn));
            if(_selectedToggleButton != null)
            {
                _selectedToggleButton.IsChecked = false;
            }
            _selectedToggleButton = btn;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleButton).IsChecked.Value)
                e.Handled = true;
        }
    }
}
