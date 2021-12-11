using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PixiEditor.Views.UserControls.Layers
{
    /// <summary>
    /// Interaction logic for LayerFolder.xaml.
    /// </summary>
    public partial class LayerGroupControl : UserControl
    {
        public Guid GroupGuid
        {
            get { return (Guid)GetValue(GroupGuidProperty); }
            set { SetValue(GroupGuidProperty, value); }
        }

        public static readonly DependencyProperty GroupGuidProperty =
            DependencyProperty.Register("GroupGuid", typeof(Guid), typeof(LayerGroupControl), new PropertyMetadata(Guid.NewGuid()));

        public LayersViewModel LayersViewModel
        {
            get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersViewModel), typeof(LayerGroupControl), new PropertyMetadata(default(LayersViewModel), LayersViewModelCallback));

        public bool IsVisibleUndoTriggerable
        {
            get { return (bool)GetValue(IsVisibleUndoTriggerableProperty); }
            set { SetValue(IsVisibleUndoTriggerableProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleUndoTriggerableProperty =
            DependencyProperty.Register("IsVisibleUndoTriggerable", typeof(bool), typeof(LayerGroupControl), new PropertyMetadata(true));

        public float GroupOpacity
        {
            get { return (float)GetValue(GroupOpacityProperty); }
            set { SetValue(GroupOpacityProperty, value); }
        }

        public static readonly DependencyProperty GroupOpacityProperty =
            DependencyProperty.Register("GroupOpacity", typeof(float), typeof(LayerGroupControl), new PropertyMetadata(1f));


        public static string LayerGroupControlDataName = typeof(LayerGroupControl).FullName;
        public static string LayerContainerDataName = typeof(LayerStructureItemContainer).FullName;

        private static void LayersViewModelCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LayerGroupControl control = (LayerGroupControl)d;
            if (e.OldValue is LayersViewModel oldVm && oldVm != e.NewValue)
            {
                oldVm.Owner.BitmapManager.StopUsingTool -= control.MouseController_StoppedRecordingChanges;
            }

            if (e.NewValue is LayersViewModel vm)
            {
                vm.Owner.BitmapManager.StopUsingTool += control.MouseController_StoppedRecordingChanges;
            }
        }

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(LayerGroupControl), new PropertyMetadata(default(string)));

        public GuidStructureItem GroupData
        {
            get { return (GuidStructureItem)GetValue(GroupDataProperty); }
            set { SetValue(GroupDataProperty, value); }
        }

        public static readonly DependencyProperty GroupDataProperty =
            DependencyProperty.Register("GroupData", typeof(GuidStructureItem), typeof(LayerGroupControl), new PropertyMetadata(default(GuidStructureItem), GroupDataChangedCallback));

        public void GeneratePreviewImage()
        {
            var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;
            var layers = doc.LayerStructure.GetGroupLayers(GroupData);
            if (layers.Count > 0)
            {
                PreviewImage = BitmapUtils.GeneratePreviewBitmap(layers, doc.Width, doc.Height, 25, 25);
            }
        }

        private static void GroupDataChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayerGroupControl)d).GeneratePreviewImage();
        }

        public WriteableBitmap PreviewImage
        {
            get { return (WriteableBitmap)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public static readonly DependencyProperty PreviewImageProperty =
            DependencyProperty.Register("PreviewImage", typeof(WriteableBitmap), typeof(LayerGroupControl), new PropertyMetadata(default(WriteableBitmap)));

        public LayerGroupControl()
        {
            InitializeComponent();
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            GeneratePreviewImage();
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            Grid item = sender as Grid;

            item.Background = LayerItem.HighlightColor;
        }

        private void Grid_CenterEnter(object sender, DragEventArgs e)
        {
            centerGrid.Background = LayerItem.HighlightColor;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;

            LayerItem.RemoveDragEffect(grid);
        }

        private void Grid_CenterLeave(object sender, DragEventArgs e)
        {
            LayerItem.RemoveDragEffect(centerGrid);
        }

        private void HandleDrop(IDataObject dataObj, Grid grid, bool above)
        {
            Guid referenceLayer = above ? GroupData.EndLayerGuid : GroupData.StartLayerGuid;
            LayerItem.RemoveDragEffect(grid);

            if (dataObj.GetDataPresent(LayerContainerDataName))
            {
                HandleLayerDrop(dataObj, above, referenceLayer, false);
            }

            if (dataObj.GetDataPresent(LayerGroupControlDataName))
            {
                HandleGroupControlDrop(dataObj, referenceLayer, above, false);
            }
        }

        private void HandleLayerDrop(IDataObject dataObj, bool above, Guid referenceLayer, bool putItInside) // step brother
        {
            var data = (LayerStructureItemContainer)dataObj.GetData(LayerContainerDataName);
            Guid group = data.Layer.LayerGuid;

            data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(group, referenceLayer, above);

            Guid? refGuid = putItInside ? GroupData?.GroupGuid : GroupData?.Parent?.GroupGuid;

            data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure.AssignParent(group, refGuid);
        }

        private void HandleGroupControlDrop(IDataObject dataObj, Guid referenceLayer, bool above, bool putItInside) // daddy
        {
            var data = (LayerGroupControl)dataObj.GetData(LayerGroupControlDataName);
            var document = data.LayersViewModel.Owner.BitmapManager.ActiveDocument;

            Guid group = data.GroupGuid;

            if (group == GroupGuid || document.LayerStructure.IsChildOf(GroupData, data.GroupData))
            {
                return;
            }

            int modifier = above ? 1 : 0;

            Layer layer = document.Layers.First(x => x.LayerGuid == referenceLayer);
            int indexOfReferenceLayer = Math.Clamp(document.Layers.IndexOf(layer) + modifier, 0, document.Layers.Count);
            MoveGroupWithTempLayer(above, document, group, indexOfReferenceLayer, putItInside);
        }

        private void MoveGroupWithTempLayer(bool above, Models.DataHolders.Document document, Guid group, int indexOfReferenceLayer, bool putItInside) // ¯\_(ツ)_/¯
        {
            // The trick here is to insert a temp layer, assign group to it, then delete it.
            Layer tempLayer = new("_temp");
            document.Layers.Insert(indexOfReferenceLayer, tempLayer);

            Guid? refGuid = putItInside ? GroupData?.GroupGuid : GroupData?.Parent?.GroupGuid;

            document.LayerStructure.AssignParent(tempLayer.LayerGuid, refGuid);
            document.MoveGroupInStructure(group, tempLayer.LayerGuid, above);
            document.LayerStructure.AssignParent(tempLayer.LayerGuid, null);
            document.RemoveLayer(tempLayer, false);
        }

        private void HandleDropInside(IDataObject dataObj, Grid grid)
        {
            Guid referenceLayer = GroupData.EndLayerGuid;
            LayerItem.RemoveDragEffect(grid);

            if (dataObj.GetDataPresent(LayerContainerDataName))
            {
                HandleLayerDrop(dataObj, true, referenceLayer, true);
            }

            if (dataObj.GetDataPresent(LayerGroupControlDataName))
            {
                HandleGroupControlDrop(dataObj, referenceLayer, true, true);
            }
        }

        private void Grid_Drop_Top(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, (Grid)sender, true);
        }

        private void Grid_Drop_Center(object sender, DragEventArgs e)
        {
            HandleDropInside(e.Data, (Grid)sender);
            LayerItem.RemoveDragEffect(centerGrid);
        }

        private void Grid_Drop_Bottom(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, (Grid)sender, false);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;
            var layer = doc.Layers.First(x => x.LayerGuid == GroupData.EndLayerGuid);
            if (doc.ActiveLayerGuid != layer.LayerGuid)
            {
                doc.SetMainActiveLayer(doc.Layers.IndexOf(layer));
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            HandleCheckboxChange(((CheckBox)e.OriginalSource).IsChecked.Value);
        }

        private void HandleCheckboxChange(bool value)
        {
            if (LayersViewModel?.Owner?.BitmapManager?.ActiveDocument != null)
            {
                var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;

                IsVisibleUndoTriggerable = value;

                var processArgs = new object[] { GroupGuid, value };
                var reverseProcessArgs = new object[] { GroupGuid, !value };

                ChangeGroupVisibilityProcess(processArgs);

                doc.UndoManager.AddUndoChange(
                new Change(
                    ChangeGroupVisibilityProcess,
                    reverseProcessArgs,
                    ChangeGroupVisibilityProcess,
                    processArgs,
                    $"Change {GroupName} visibility"), false);
            }
        }

        private void ChangeGroupVisibilityProcess(object[] args)
        {
            var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;
            if (args.Length == 2 &&
                args[0] is Guid groupGuid &&
                args[1] is bool value
                && doc != null)
            {
                var group = doc.LayerStructure.GetGroupByGuid(groupGuid);

                group.IsVisible = value;
                var layers = doc.LayerStructure.GetGroupLayers(group);

                foreach (var layer in layers)
                {
                    layer.IsVisible = layer.IsVisible;
                }

                IsVisibleUndoTriggerable = value;
            }
        }

        private void GroupControl_DragEnter(object sender, DragEventArgs e)
        {
            middleDropGrid.Visibility = Visibility.Visible;
        }

        private void GroupControl_DragLeave(object sender, DragEventArgs e)
        {
            middleDropGrid.Visibility = Visibility.Collapsed;
        }
    }
}
