using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls.Layers
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(LayersManager), new PropertyMetadata(0));


        public ObservableCollection<object> LayerTreeRoot
        {
            get { return (ObservableCollection<object>)GetValue(LayerTreeRootProperty); }
            set { SetValue(LayerTreeRootProperty, value); }
        }

        public static readonly DependencyProperty LayerTreeRootProperty =
            DependencyProperty.Register(
                "LayerTreeRoot",
                typeof(ObservableCollection<object>),
                typeof(LayersManager),
                new PropertyMetadata(default(ObservableCollection<object>)));
        public LayersViewModel LayerCommandsViewModel
        {
            get { return (LayersViewModel)GetValue(LayerCommandsViewModelProperty); }
            set { SetValue(LayerCommandsViewModelProperty, value); }
        }

        public static readonly DependencyProperty LayerCommandsViewModelProperty =
            DependencyProperty.Register("LayerCommandsViewModel", typeof(LayersViewModel), typeof(LayersManager), new PropertyMetadata(default(LayersViewModel), ViewModelChanged));

        public bool OpacityInputEnabled
        {
            get { return (bool)GetValue(OpacityInputEnabledProperty); }
            set { SetValue(OpacityInputEnabledProperty, value); }
        }

        public static readonly DependencyProperty OpacityInputEnabledProperty =
            DependencyProperty.Register("OpacityInputEnabled", typeof(bool), typeof(LayersManager), new PropertyMetadata(false));

        public LayersManager()
        {
            InitializeComponent();
        }

        private static void ViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is LayersViewModel vm)
            {
                LayersManager manager = (LayersManager)d;
                vm.Owner.BitmapManager.AddPropertyChangedCallback(nameof(vm.Owner.BitmapManager.ActiveDocument), () =>
                {
                    var doc = vm.Owner.BitmapManager.ActiveDocument;
                    if (doc != null)
                    {
                        if (doc.ActiveLayer != null)
                        {
                            manager.SetActiveLayerAsSelectedItem(doc);
                        }
                        doc.AddPropertyChangedCallback(nameof(doc.ActiveLayer), () =>
                        {
                            manager.SetActiveLayerAsSelectedItem(doc);
                        });
                    }
                });
            }
        }

        private void SetActiveLayerAsSelectedItem(Document doc)
        {
            SelectedItem = doc.ActiveLayer;
            SetInputOpacity(SelectedItem);
        }

        private void SetInputOpacity(object item)
        {
            if (item is Layer layer)
            {
                numberInput.Value = layer.Opacity * 100f;
            }
            else if (item is LayerStructureItemContainer container)
            {
                numberInput.Value = container.Layer.Opacity * 100f;
            }
            else if (item is LayerGroup group)
            {
                numberInput.Value = group.StructureData.Opacity * 100f;
            }
            else if (item is LayerGroupControl groupControl)
            {
                numberInput.Value = groupControl.GroupData.Opacity * 100f;
            }
        }

        private void LayerStructureItemContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerStructureItemContainer container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
            }
        }

        private void HandleGroupOpacityChange(GuidStructureItem group, float value)
        {
            if (LayerCommandsViewModel.Owner?.BitmapManager?.ActiveDocument != null)
            {
                var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;

                if (group.Opacity == value)
                    return;

                var processArgs = new object[] { group.GroupGuid, value };
                var reverseProcessArgs = new object[] { group.GroupGuid, group.Opacity };

                ChangeGroupOpacityProcess(processArgs);

                LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure.ExpandParentGroups(group);

                doc.UndoManager.AddUndoChange(
                new Change(
                    ChangeGroupOpacityProcess,
                    reverseProcessArgs,
                    ChangeGroupOpacityProcess,
                    processArgs,
                    $"Change {group.Name} opacity"), false);
            }
        }

        private void ChangeGroupOpacityProcess(object[] processArgs)
        {
            if (processArgs.Length > 0 && processArgs[0] is Guid groupGuid && processArgs[1] is float opacity)
            {
                var structure = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure;
                var group = structure.GetGroupByGuid(groupGuid);
                group.Opacity = opacity;
                var layers = structure.GetGroupLayers(group);
                layers.ForEach(x => x.Opacity = x.Opacity); // This might seems stupid, but it raises property changed, without setting any value. This is used to trigger converters that use group opacity
                numberInput.Value = opacity * 100;
            }
        }

        private void LayerGroup_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is LayerGroupControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
            }
        }

        private void NumberInput_LostFocus(object sender, RoutedEventArgs e)
        {
            float val = numberInput.Value / 100f;

            object item = SelectedItem;

            if (item is Layer || item is LayerStructureItemContainer)
            {

                Layer layer = null;

                if (item is Layer lr)
                {
                    layer = lr;
                }
                else if (item is LayerStructureItemContainer container)
                {
                    layer = container.Layer;
                }

                HandleLayerOpacityChange(val, layer);
            }
            else if (item is LayerGroup group)
            {
                HandleGroupOpacityChange(group.StructureData, val);
            }
            else if (item is LayerGroupControl groupControl)
            {
                HandleGroupOpacityChange(groupControl.GroupData, val);
            }
        }

        private void HandleLayerOpacityChange(float val, Layer layer)
        {
            float oldOpacity = layer.Opacity;
            if (oldOpacity == val)
                return;

            var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;

            doc.RaisePropertyChange(nameof(doc.LayerStructure));

            layer.OpacityUndoTriggerable = val;

            doc.LayerStructure.ExpandParentGroups(layer.LayerGuid);

            doc.RaisePropertyChange(nameof(doc.LayerStructure));

            UndoManager undoManager = doc.UndoManager;


            undoManager.AddUndoChange(
                new Change(
                    UpdateNumberInputLayerOpacityProcess,
                    new object[] { oldOpacity },
                    UpdateNumberInputLayerOpacityProcess,
                    new object[] { val }));
            undoManager.SquashUndoChanges(2);
        }

        private void UpdateNumberInputLayerOpacityProcess(object[] args)
        {
            if (args.Length > 0 && args[0] is float opacity)
            {
                numberInput.Value = opacity * 100;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetInputOpacity(SelectedItem);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            dropBorder.BorderBrush = Brushes.Transparent;

            if (e.Data.GetDataPresent(LayerGroupControl.LayerContainerDataName))
            {
                HandleLayerDrop(e.Data);
            }

            if (e.Data.GetDataPresent(LayerGroupControl.LayerGroupControlDataName))
            {
                HandleGroupControlDrop(e.Data);
            }
        }

        private void HandleLayerDrop(IDataObject data)
        {
            var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
            if (doc.Layers.Count == 0) return;

            var layerContainer = (LayerStructureItemContainer)data.GetData(LayerGroupControl.LayerContainerDataName);
            var refLayer = doc.Layers[0].LayerGuid;
            doc.MoveLayerInStructure(layerContainer.Layer.LayerGuid, refLayer);
            doc.LayerStructure.AssignParent(layerContainer.Layer.LayerGuid, null);
        }

        private void HandleGroupControlDrop(IDataObject data)
        {
            var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
            var groupContainer = (LayerGroupControl)data.GetData(LayerGroupControl.LayerGroupControlDataName);
            doc.LayerStructure.MoveGroup(groupContainer.GroupGuid, 0);
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            ((Border)sender).BorderBrush = LayerItem.HighlightColor;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            ((Border)sender).BorderBrush = Brushes.Transparent;
        }

        private void SelectActiveItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectedItem = sender;
        }
    }
}
