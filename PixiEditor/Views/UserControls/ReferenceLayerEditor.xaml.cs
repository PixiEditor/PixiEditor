using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ReferenceLayerEditor.xaml
    /// </summary>
    public partial class ReferenceLayerEditor : UserControl
    {
        private static readonly DependencyProperty ReferenceLayerProperty =
            DependencyProperty.Register(nameof(ReferenceLayer), typeof(ReferenceLayer), typeof(ReferenceLayerEditor));

        public ReferenceLayer ReferenceLayer
        {
            get => (ReferenceLayer)GetValue(ReferenceLayerProperty);
            set => SetValue(ReferenceLayerProperty, value);
        }

        public RelayCommand UpdateReferenceLayerCommand { get; set; }

        private static readonly DependencyProperty ToggleCommandProperty =
            DependencyProperty.Register(nameof(ToggleCommand), typeof(RelayCommand), typeof(ReferenceLayerEditor));

        public RelayCommand ToggleCommand
        {
            get => (RelayCommand)GetValue(ToggleCommandProperty);
            set => SetValue(ToggleCommandProperty, value);
        }

        public ReferenceLayerEditor()
        {
            UpdateReferenceLayerCommand = new RelayCommand(UpdateReferenceLayer);
            InitializeComponent();
        }

        private void UpdateReferenceLayer(object obj)
        {
            ReferenceLayer.Update();
        }
    }
}
