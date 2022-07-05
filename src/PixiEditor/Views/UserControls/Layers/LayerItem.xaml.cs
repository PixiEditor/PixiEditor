using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers;

namespace PixiEditor.Views.UserControls.Layers;

/// <summary>
/// Interaction logic for LayerItem.xaml.
/// </summary>
internal partial class LayerItem : UserControl
{
    public static Brush HighlightColor = Brushes.Blue;

    public LayerItem()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty IsRenamingProperty = DependencyProperty.Register(
        nameof(IsRenaming), typeof(bool), typeof(LayerItem), new PropertyMetadata(default(bool)));

    public bool IsRenaming
    {
        get { return (bool)GetValue(IsRenamingProperty); }
        set { SetValue(IsRenamingProperty, value); }
    }

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive), typeof(bool), typeof(LayerItem), new PropertyMetadata(default(bool)));

    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set { SetValue(IsActiveProperty, value); }
    }

    public static readonly DependencyProperty SetActiveLayerCommandProperty = DependencyProperty.Register(
        nameof(SetActiveLayerCommand), typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

    public RelayCommand SetActiveLayerCommand
    {
        get { return (RelayCommand)GetValue(SetActiveLayerCommandProperty); }
        set { SetValue(SetActiveLayerCommandProperty, value); }
    }

    public static readonly DependencyProperty LayerIndexProperty = DependencyProperty.Register(
        nameof(LayerIndex), typeof(int), typeof(LayerItem), new PropertyMetadata(default(int)));

    public int LayerIndex
    {
        get { return (int)GetValue(LayerIndexProperty); }
        set { SetValue(LayerIndexProperty, value); }
    }

    public static readonly DependencyProperty LayerNameProperty = DependencyProperty.Register(
        nameof(LayerName), typeof(string), typeof(LayerItem), new PropertyMetadata(default(string)));

    public string LayerName
    {
        get { return (string)GetValue(LayerNameProperty); }
        set { SetValue(LayerNameProperty, value); }
    }

    public Guid LayerGuid
    {
        get { return (Guid)GetValue(LayerGuidProperty); }
        set { SetValue(LayerGuidProperty, value); }
    }

    public static readonly DependencyProperty LayerGuidProperty =
        DependencyProperty.Register(nameof(LayerGuid), typeof(Guid), typeof(LayerItem), new PropertyMetadata(default(Guid)));

    public static readonly DependencyProperty ControlButtonsVisibleProperty = DependencyProperty.Register(
        nameof(ControlButtonsVisible), typeof(Visibility), typeof(LayerItem), new PropertyMetadata(System.Windows.Visibility.Hidden));

    public string LayerColor
    {
        get { return (string)GetValue(LayerColorProperty); }
        set { SetValue(LayerColorProperty, value); }
    }

    public static readonly DependencyProperty LayerColorProperty =
        DependencyProperty.Register(nameof(LayerColor), typeof(string), typeof(LayerItem), new PropertyMetadata("#00000000"));

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
        DependencyProperty.Register(nameof(MoveToBackCommand), typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

    public static readonly DependencyProperty MoveToFrontCommandProperty = DependencyProperty.Register(
        nameof(MoveToFrontCommand), typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

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
        Grid item = sender as Grid;
        RemoveDragEffect(item);

        if (e.Data.GetDataPresent(LayerGroupControl.LayerContainerDataName))
        {
            var data = (LayerStructureItemContainer)e.Data.GetData(LayerGroupControl.LayerContainerDataName);
            //Guid layer = data.Layer.GuidValue;
            //var doc = data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;

            /*doc.MoveLayerInStructure(layer, LayerGuid, above);
            if (dropInParentFolder)
            {
                Guid? groupGuid = doc.LayerStructure.GetGroupByLayer(layer)?.Parent?.GroupGuid;
                doc.LayerStructure.AssignParent(layer, groupGuid);
            }*/
        }

        if (e.Data.GetDataPresent(LayerGroupControl.LayerGroupControlDataName))
        {
            var data = (LayerGroupControl)e.Data.GetData(LayerGroupControl.LayerGroupControlDataName);
            //Guid folder = data.GroupGuid;

            //var document = data.LayersViewModel.Owner.BitmapManager.ActiveDocument;

            /*var parentGroup = document.LayerStructure.GetGroupByLayer(LayerGuid);

            if (parentGroup == data.GroupData || document.LayerStructure.IsChildOf(parentGroup, data.GroupData))
            {
                return;
            }

            document.MoveGroupInStructure(folder, LayerGuid, above);*/
        }
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
