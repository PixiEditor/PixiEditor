using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        private object cachedItem = null;

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
                new PropertyMetadata(default(ObservableCollection<object>), ItemsChanged));

        private static void ItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var items = (ObservableCollection<object>)e.NewValue;
            LayersManager manager = (LayersManager)d;
            if (items != null && items.Count > 0 && ((ObservableCollection<object>)e.OldValue).Count == 0)
            {
                var item = items[0];
                manager.cachedItem = item;
                var numberInput = manager.numberInput;
                SetInputOpacity(item, numberInput);
            }
        }

        private static void SetInputOpacity(object item, NumberInput numberInput)
        {
            if (item is Layer layer)
            {
                numberInput.Value = layer.Opacity * 100f;
            }
            else if (item is LayerGroup group)
            {
                numberInput.Value = group.StructureData.Opacity * 100f;
            }
        }

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

        private void LayerGroup_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerGroupControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(container, container, DragDropEffects.Move);
            }
        }

        private void NumberInput_LostFocus(object sender, RoutedEventArgs e)
        {
            float val = numberInput.Value / 100f;

            object item = treeView.SelectedItem;

            if (item == null && cachedItem != null)
            {
                item = cachedItem;
            }

            if (item is Layer layer)
            {
                layer.Opacity = val;
            }
            else if(item is LayerGroup group)
            {
                LayerStructure structure = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure;
                var groupData = structure.GetGroupByGuid(group.GroupGuid);
                groupData.Opacity = val;

                var layers = structure.GetGroupLayers(groupData);
                layers.ForEach(x => x.Opacity = x.Opacity); // This might seems stupid, but it raises property changed, without setting any value. This is used to trigger converters that use group opacity
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetInputOpacity(treeView.SelectedItem, numberInput);
        }
    }
}