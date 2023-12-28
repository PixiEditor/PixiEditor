using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;
#nullable enable
internal partial class LayerControl : UserControl
{
    public static readonly DependencyProperty LayerProperty =
        DependencyProperty.Register(nameof(Layer), typeof(LayerViewModel), typeof(LayerControl), new(null));

    private readonly Brush? highlightColor;

    public LayerViewModel Layer
    {
        get => (LayerViewModel)GetValue(LayerProperty);
        set => SetValue(LayerProperty, value);
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

    public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(
        nameof(Manager), typeof(LayersManager), typeof(LayerControl), new PropertyMetadata(default(LayersManager)));

    public LayersManager Manager
    {
        get { return (LayersManager)GetValue(ManagerProperty); }
        set { SetValue(ManagerProperty, value); }
    }
    
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


    private MouseUpdateController? mouseUpdateController;
    
    public LayerControl()
    {
        InitializeComponent();
        Loaded += LayerControl_Loaded;
        Unloaded += LayerControl_Unloaded;
        highlightColor = (Brush?)App.Current.Resources["SoftSelectedLayerColor"];
    }

    private void LayerControl_Unloaded(object sender, RoutedEventArgs e)
    { 
        mouseUpdateController?.Dispose();
    }

    private void LayerControl_Loaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, Manager.LayerControl_MouseMove);
    }

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

    private void Grid_DragEnter(object? sender, DragEventArgs e)
    {
        Grid? item = sender as Grid;
        if (item is not null)
            item.Background = highlightColor;
    }

    private void Grid_DragLeave(object? sender, DragEventArgs e)
    {
        Grid? item = sender as Grid;
        if (item is not null)
            RemoveDragEffect(item);
    }

    public static Guid? ExtractMemberGuid(IDataObject droppedMemberDataObject)
    {
        object droppedLayer = droppedMemberDataObject.GetData(FolderControl.LayerControlDataName);
        object droppedFolder = droppedMemberDataObject.GetData(FolderControl.FolderControlDataName);
        if (droppedLayer is LayerControl layer)
            return layer.Layer.GuidValue;
        else if (droppedFolder is FolderControl folder)
            return folder.Folder.GuidValue;
        return null;
    }

    private void HandleDrop(IDataObject dataObj, StructureMemberPlacement placement)
    {
        if (placement == StructureMemberPlacement.Inside)
            return;
        Guid? droppedMemberGuid = ExtractMemberGuid(dataObj);
        if (droppedMemberGuid is null)
            return;
        Layer.Document.Operations.MoveStructureMember((Guid)droppedMemberGuid, Layer.GuidValue, placement);
    }

    private void Grid_Drop_Top(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        HandleDrop(e.Data, StructureMemberPlacement.Above);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        HandleDrop(e.Data, StructureMemberPlacement.Below);
    }

    private void Grid_Drop_Below(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        HandleDrop(e.Data, StructureMemberPlacement.BelowOutsideFolder);
    }

    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        editableTextBlock.EnableEditing();
    }

    private void MaskMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Layer is not null)
            Layer.ShouldDrawOnMask = true;
    }

    private void LayerMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Layer is not null)
            Layer.ShouldDrawOnMask = false;
    }
}
