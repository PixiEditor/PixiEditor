using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.Layers;

namespace PixiEditor.Views.UserControls.Layers
{
    /// <summary>
    /// Interaction logic for RawLayersViewer.xaml.
    /// </summary>
    public partial class RawLayersViewer : UserControl
    {
        public ObservableCollection<Layer> Layers
        {
            get { return (ObservableCollection<Layer>)GetValue(LayersProperty); }
            set { SetValue(LayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Layers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersProperty =
            DependencyProperty.Register(
                "Layers",
                typeof(ObservableCollection<Layer>),
                typeof(RawLayersViewer),
                new PropertyMetadata(default(ObservableCollection<Layer>)));

        public LayerStructure Structure
        {
            get { return (LayerStructure)GetValue(StructureProperty); }
            set { SetValue(StructureProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Structure.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StructureProperty =
            DependencyProperty.Register("Structure", typeof(LayerStructure), typeof(RawLayersViewer), new PropertyMetadata(default(LayerStructure)));

        public RawLayersViewer()
        {
            InitializeComponent();
        }
    }
}