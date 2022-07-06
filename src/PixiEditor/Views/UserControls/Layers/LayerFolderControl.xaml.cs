using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls.Layers;

internal partial class LayerFolderControl : UserControl
{
    public static readonly DependencyProperty GroupGuidProperty =
        DependencyProperty.Register(nameof(FolderGuid), typeof(Guid), typeof(LayerFolderControl), new PropertyMetadata(Guid.NewGuid()));
    public Guid FolderGuid
    {
        get => (Guid)GetValue(GroupGuidProperty);
        set => SetValue(GroupGuidProperty, value);
    }

    public static readonly DependencyProperty LayersViewModelProperty =
        DependencyProperty.Register(nameof(LayersViewModel), typeof(LayersViewModel), typeof(LayerFolderControl), new PropertyMetadata(default(LayersViewModel)));
    public LayersViewModel LayersViewModel
    {
        get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
        set { SetValue(LayersViewModelProperty, value); }
    }

    public bool IsVisibleUndoTriggerable
    {
        get { return (bool)GetValue(IsVisibleUndoTriggerableProperty); }
        set { SetValue(IsVisibleUndoTriggerableProperty, value); }
    }

    public static readonly DependencyProperty IsVisibleUndoTriggerableProperty =
        DependencyProperty.Register(nameof(IsVisibleUndoTriggerable), typeof(bool), typeof(LayerFolderControl), new PropertyMetadata(true));

    public float GroupOpacity
    {
        get { return (float)GetValue(GroupOpacityProperty); }
        set { SetValue(GroupOpacityProperty, value); }
    }

    public static readonly DependencyProperty GroupOpacityProperty =
        DependencyProperty.Register(nameof(GroupOpacity), typeof(float), typeof(LayerFolderControl), new PropertyMetadata(1f));


    public static string LayerGroupControlDataName = typeof(LayerFolderControl).FullName;
    public static string LayerContainerDataName = typeof(LayerStructureItemContainer).FullName;

    public string GroupName
    {
        get { return (string)GetValue(GroupNameProperty); }
        set { SetValue(GroupNameProperty, value); }
    }

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(LayerFolderControl), new PropertyMetadata(default(string)));

    public WriteableBitmap PreviewImage
    {
        get { return (WriteableBitmap)GetValue(PreviewImageProperty); }
        set { SetValue(PreviewImageProperty, value); }
    }

    public static readonly DependencyProperty PreviewImageProperty =
        DependencyProperty.Register(nameof(PreviewImage), typeof(WriteableBitmap), typeof(LayerFolderControl), new PropertyMetadata(default(WriteableBitmap)));

    public LayerFolderControl()
    {
        InitializeComponent();
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
        LayerItem.RemoveDragEffect(centerGrid);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        //HandleDrop(e.Data, (Grid)sender, false);
    }

    private void Border_MouseDown(object sender, MouseButtonEventArgs e)
    {
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
