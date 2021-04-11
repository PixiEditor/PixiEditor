using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
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

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupGuidProperty =
            DependencyProperty.Register("GroupGuid", typeof(Guid), typeof(LayerGroupControl), new PropertyMetadata(Guid.NewGuid()));

        public LayersViewModel LayersViewModel
        {
            get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersViewModel), typeof(LayerGroupControl), new PropertyMetadata(default(LayersViewModel), LayersViewModelCallback));

        public bool IsVisibleUndoTriggerable
        {
            get { return (bool)GetValue(IsVisibleUndoTriggerableProperty); }
            set { SetValue(IsVisibleUndoTriggerableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsVisibleUndoTriggerable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleUndoTriggerableProperty =
            DependencyProperty.Register("IsVisibleUndoTriggerable", typeof(bool), typeof(LayerGroupControl), new PropertyMetadata(true, IsVisibleChangedCallback));

        private static void IsVisibleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LayerGroupControl control = (LayerGroupControl)d;
            var doc = control.LayersViewModel.Owner.BitmapManager.ActiveDocument;
            var layers = doc.LayerStructure.GetGroupLayers(control.GroupData);

            foreach (var layer in layers)
            {
                layer.IsVisible = (bool)e.NewValue;
            }

            doc.UndoManager.AddUndoChange(
                new Change(
                    nameof(IsVisibleUndoTriggerable), 
                    e.OldValue,
                    e.NewValue,
                    $"Change {control.GroupName} visiblity",
                    control), true);
        }

        private static void LayersViewModelCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LayerGroupControl control = (LayerGroupControl)d;
            if(e.OldValue is LayersViewModel oldVm && oldVm != e.NewValue)
            {
                oldVm.Owner.BitmapManager.MouseController.StoppedRecordingChanges -= control.MouseController_StoppedRecordingChanges;
            }

            if(e.NewValue is LayersViewModel vm)
            {
                vm.Owner.BitmapManager.MouseController.StoppedRecordingChanges += control.MouseController_StoppedRecordingChanges;
            }
        }

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(LayerGroupControl), new PropertyMetadata(default(string)));

        public GuidStructureItem GroupData
        {
            get { return (GuidStructureItem)GetValue(GroupDataProperty); }
            set { SetValue(GroupDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupDataProperty =
            DependencyProperty.Register("GroupData", typeof(GuidStructureItem), typeof(LayerGroupControl), new PropertyMetadata(default(GuidStructureItem), GroupDataChangedCallback));

        public void GeneratePreviewImage()
        {
            var layers = LayersViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure.GetGroupLayers(GroupData);
            PreviewImage = BitmapUtils.GeneratePreviewBitmap(layers, 25, 25, true);
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

        // Using a DependencyProperty as the backing store for PreviewImage.  This enables animation, styling, binding, etc...
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

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;

            LayerItem.RemoveDragEffect(grid);
        }

        private void HandleDrop(IDataObject dataObj, bool above)
        {
            Guid referenceLayer = above ? (Guid)GroupData.EndLayerGuid : (Guid)GroupData.StartLayerGuid;

            if (dataObj.GetDataPresent("PixiEditor.Views.UserControls.LayerStructureItemContainer"))
            {
                var data = (LayerStructureItemContainer)dataObj.GetData("PixiEditor.Views.UserControls.LayerStructureItemContainer");
                Guid group = data.Layer.LayerGuid;

                data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(group, referenceLayer, above);
                data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure.AssignParent(group, GroupData.Parent);
            }

            if (dataObj.GetDataPresent("PixiEditor.Views.UserControls.LayerGroupControl"))
            {
                var data = (LayerGroupControl)dataObj.GetData("PixiEditor.Views.UserControls.LayerGroupControl");
                var document = data.LayersViewModel.Owner.BitmapManager.ActiveDocument;

                Guid group = data.GroupGuid;

                if(group == GroupGuid || document.LayerStructure.IsChildOf(GroupData, data.GroupData))
                {
                    return;
                }

                int modifier = above ? 1 : -1;

                Layer layer = document.Layers.First(x => x.LayerGuid == referenceLayer);
                int indexOfReferenceLayer = Math.Clamp(document.Layers.IndexOf(layer) + modifier, 0, document.Layers.Count);

                Layer tempLayer = new("_temp");
                document.Layers.Insert(indexOfReferenceLayer, tempLayer);

                document.LayerStructure.AssignParent(tempLayer.LayerGuid, GroupData.Parent);
                document.MoveFolderInStructure(group, tempLayer.LayerGuid, above);
                document.LayerStructure.AssignParent(tempLayer.LayerGuid, null);
                document.RemoveLayer(tempLayer, false);
            }
        }

        private void Grid_Drop_Top(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, true);
        }

        private void Grid_Drop_Bottom(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, false);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;
            doc.SetMainActiveLayer(doc.Layers.IndexOf(doc.Layers.First(x => x.LayerGuid == GroupData.EndLayerGuid)));
        }
    }
}