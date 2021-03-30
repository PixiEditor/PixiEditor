using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        public ObservableCollection<object> LayerTreeRoot
        {
            get { return (ObservableCollection<object>)GetValue(LayerTreeRootProperty); }
            set { SetValue(LayerTreeRootProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayerTreeRoot.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerTreeRootProperty =
            DependencyProperty.Register(
                "LayerTreeRoot",
                typeof(ObservableCollection<object>),
                typeof(LayersManager),
                new PropertyMetadata(default(ObservableCollection<object>)));

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

        private void LayerStructureItemContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerStructureItemContainer container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(container, container, DragDropEffects.Move);
            }
        }

        private void LayerFolder_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerGroupControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(container, container, DragDropEffects.Move);
            }
        }
    }
}