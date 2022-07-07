using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;

internal partial class FolderControl : UserControl
{
    public static readonly DependencyProperty FolderProperty =
        DependencyProperty.Register(nameof(Folder), typeof(FolderViewModel), typeof(FolderControl), new(null));

    public FolderViewModel Folder
    {
        get => (FolderViewModel)GetValue(FolderProperty);
        set => SetValue(FolderProperty, value);
    }

    public static string FolderControlDataName = typeof(FolderControl).FullName;

    public FolderControl()
    {
        InitializeComponent();
    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        Grid item = sender as Grid;

        item.Background = LayerControl.HighlightColor;
    }

    private void Grid_CenterEnter(object sender, DragEventArgs e)
    {
        centerGrid.Background = LayerControl.HighlightColor;
    }

    private void Grid_DragLeave(object sender, DragEventArgs e)
    {
        Grid grid = (Grid)sender;

        LayerControl.RemoveDragEffect(grid);
    }

    private void Grid_CenterLeave(object sender, DragEventArgs e)
    {
        LayerControl.RemoveDragEffect(centerGrid);
    }

    /*
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
        Guid group = data.Layer.GuidValue;

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

        Layer layer = document.Layers.First(x => x.GuidValue == referenceLayer);
        int indexOfReferenceLayer = Math.Clamp(document.Layers.IndexOf(layer) + modifier, 0, document.Layers.Count);
        MoveGroupWithTempLayer(above, document, group, indexOfReferenceLayer, putItInside);
    }

    private void MoveGroupWithTempLayer(bool above, Models.DataHolders.Document document, Guid group, int indexOfReferenceLayer, bool putItInside) // ¯\_(ツ)_/¯
    {
        // The trick here is to insert a temp layer, assign group to it, then delete it.
        Layer tempLayer = new("_temp", document.Width, document.Height);
        document.Layers.Insert(indexOfReferenceLayer, tempLayer);

        Guid? refGuid = putItInside ? GroupData?.GroupGuid : GroupData?.Parent?.GroupGuid;

        document.LayerStructure.AssignParent(tempLayer.GuidValue, refGuid);
        document.MoveGroupInStructure(group, tempLayer.GuidValue, above);
        document.LayerStructure.AssignParent(tempLayer.GuidValue, null);
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
    }*/

    private void Grid_Drop_Top(object sender, DragEventArgs e)
    {
        //HandleDrop(e.Data, (Grid)sender, true);
    }

    private void Grid_Drop_Center(object sender, DragEventArgs e)
    {
        //HandleDropInside(e.Data, (Grid)sender);
        LayerControl.RemoveDragEffect(centerGrid);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        //HandleDrop(e.Data, (Grid)sender, false);
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Folder?.Document.SetSelectedMember(Folder.GuidValue);
        /*
            var doc = LayersViewModel.Owner.BitmapManager.ActiveDocument;
            var layer = doc.Layers.First(x => x.GuidValue == GroupData.EndLayerGuid);
            if (doc.ActiveLayerGuid != layer.GuidValue)
            {
                doc.SetMainActiveLayer(doc.Layers.IndexOf(layer));
            }*/
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        //HandleCheckboxChange(((CheckBox)e.OriginalSource).IsChecked.Value);
    }
    /*
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
    }*/

    private void FolderControl_DragEnter(object sender, DragEventArgs e)
    {
        middleDropGrid.Visibility = Visibility.Visible;
    }

    private void FolderControl_DragLeave(object sender, DragEventArgs e)
    {
        middleDropGrid.Visibility = Visibility.Collapsed;
    }
}
