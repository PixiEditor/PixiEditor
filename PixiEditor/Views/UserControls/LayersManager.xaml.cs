using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        public ObservableCollection<LayerStructureItem> StructuredLayers
        {
            get { return (ObservableCollection<LayerStructureItem>)GetValue(StructuredLayersProperty); }
            set { SetValue(StructuredLayersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StructuredLayers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StructuredLayersProperty =
            DependencyProperty.Register(
                "StructuredLayers",
                typeof(ObservableCollection<LayerStructureItem>),
                typeof(LayersManager),
                new PropertyMetadata(default(ObservableCollection<LayerStructureItem>)));

        public float LayerOpacity
        {
            get { return (float)GetValue(LayerOpacityProperty); }
            set { SetValue(LayerOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerOpacityProperty =
            DependencyProperty.Register("LayerOpacity", typeof(float), typeof(LayersManager), new PropertyMetadata(0f));

        public LayersViewModel LayerCommandsViewModel
        {
            get { return (LayersViewModel)GetValue(LayerCommandsViewModelProperty); }
            set { SetValue(LayerCommandsViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerCommandsViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerCommandsViewModelProperty =
            DependencyProperty.Register("LayerCommandsViewModel", typeof(LayersViewModel), typeof(LayersManager), new PropertyMetadata(default(LayersViewModel)));
      
        public bool OpacityInputEnabled
        {
            get { return (bool)GetValue(OpacityInputEnabledProperty); }
            set { SetValue(OpacityInputEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpacityInputEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityInputEnabledProperty =
            DependencyProperty.Register("OpacityInputEnabled", typeof(bool), typeof(LayersManager), new PropertyMetadata(false));

        public LayersManager()
        {
            InitializeComponent();
        }
    }
}