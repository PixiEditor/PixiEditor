using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;

internal partial class LayerControl : UserControl
{
    public static Brush HighlightColor = Brushes.Blue;

    public static readonly DependencyProperty LayerProperty =
        DependencyProperty.Register(nameof(Layer), typeof(LayerViewModel), typeof(LayerControl), new(null));

    public LayerViewModel Layer
    {
        get => (LayerViewModel)GetValue(LayerProperty);
        set => SetValue(LayerProperty, value);
    }

    public LayerControl()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty SetActiveLayerCommandProperty = DependencyProperty.Register(
        nameof(SetActiveLayerCommand), typeof(RelayCommand), typeof(LayerControl), new PropertyMetadata(default(RelayCommand)));

    public RelayCommand SetActiveLayerCommand
    {
        get { return (RelayCommand)GetValue(SetActiveLayerCommandProperty); }
        set { SetValue(SetActiveLayerCommandProperty, value); }
    }

    public static readonly DependencyProperty ControlButtonsVisibleProperty = DependencyProperty.Register(
        nameof(ControlButtonsVisible), typeof(Visibility), typeof(LayerControl), new PropertyMetadata(System.Windows.Visibility.Hidden));

    public string LayerColor
    {
        get { return (string)GetValue(LayerColorProperty); }
        set { SetValue(LayerColorProperty, value); }
    }

    public static readonly DependencyProperty LayerColorProperty =
        DependencyProperty.Register(nameof(LayerColor), typeof(string), typeof(LayerControl), new PropertyMetadata("#00000000"));

    public Visibility ControlButtonsVisible
    {
        get { return (Visibility)GetValue(ControlButtonsVisibleProperty); }
        set { SetValue(ControlButtonsVisibleProperty, value); }
    }

    public RelayCommand MoveToBackCommand
    {
        get { return (RelayCommand)GetValue(MoveToBackCommandProperty); }
        set { SetValue(MoveToBackCommandProperty, value); }
    }

    public static readonly DependencyProperty MoveToBackCommandProperty =
        DependencyProperty.Register(nameof(MoveToBackCommand), typeof(RelayCommand), typeof(LayerControl), new PropertyMetadata(default(RelayCommand)));

    public static readonly DependencyProperty MoveToFrontCommandProperty = DependencyProperty.Register(
        nameof(MoveToFrontCommand), typeof(RelayCommand), typeof(LayerControl), new PropertyMetadata(default(RelayCommand)));

    public RelayCommand MoveToFrontCommand
    {
        get { return (RelayCommand)GetValue(MoveToFrontCommandProperty); }
        set { SetValue(MoveToFrontCommandProperty, value); }
    }

    public static void RemoveDragEffect(Grid grid)
    {
        grid.Background = Brushes.Transparent;
    }

    private void LayerItem_OnMouseEnter(object sender, MouseEventArgs e)
    {
        ControlButtonsVisible = Visibility.Visible;
    }

    private void LayerItem_OnMouseLeave(object sender, MouseEventArgs e)
    {
        ControlButtonsVisible = Visibility.Hidden;

    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        Grid item = sender as Grid;

        item.Background = HighlightColor;
    }

    private void Grid_DragLeave(object sender, DragEventArgs e)
    {
        Grid item = sender as Grid;

        RemoveDragEffect(item);
    }

    private void HandleGridDrop(object sender, DragEventArgs e, bool above, bool dropInParentFolder = false)
    {
        /*
        Grid item = sender as Grid;
        RemoveDragEffect(item);

        if (e.Data.GetDataPresent(FolderControl.LayerContainerDataName))
        {
            var data = (LayerControlContainer)e.Data.GetData(FolderControl.LayerContainerDataName);
            //Guid layer = data.Layer.GuidValue;
            //var doc = data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;

            /*doc.MoveLayerInStructure(layer, LayerGuid, above);
            if (dropInParentFolder)
            {
                Guid? groupGuid = doc.LayerStructure.GetGroupByLayer(layer)?.Parent?.GroupGuid;
                doc.LayerStructure.AssignParent(layer, groupGuid);
            }
        }

        if (e.Data.GetDataPresent(FolderControl.FolderControlDataName))
        {
            var data = (FolderControl)e.Data.GetData(FolderControl.FolderControlDataName);
            //Guid folder = data.GroupGuid;

            //var document = data.LayersViewModel.Owner.BitmapManager.ActiveDocument;

            /*var parentGroup = document.LayerStructure.GetGroupByLayer(LayerGuid);

            if (parentGroup == data.GroupData || document.LayerStructure.IsChildOf(parentGroup, data.GroupData))
            {
                return;
            }

            document.MoveGroupInStructure(folder, LayerGuid, above);
        }
*/
    }

    private void Grid_Drop_Top(object sender, DragEventArgs e)
    {
        HandleGridDrop(sender, e, true);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        HandleGridDrop(sender, e, false);
    }

    private void Grid_Drop_Below(object sender, DragEventArgs e)
    {
        HandleGridDrop(sender, e, false, true);
    }
}
