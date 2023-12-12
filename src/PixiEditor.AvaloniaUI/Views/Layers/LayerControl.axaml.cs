using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Layers;
#nullable enable
internal partial class LayerControl : UserControl
{
    public static string? LayerControlDataName = typeof(LayerControl).FullName;

    public static readonly StyledProperty<LayerViewModel> LayerProperty =
        AvaloniaProperty.Register<LayerControl, LayerViewModel>(nameof(Layer));

    public LayerViewModel Layer
    {
        get => GetValue(LayerProperty);
        set => SetValue(LayerProperty, value);
    }

    private readonly IBrush? highlightColor;

    public static readonly StyledProperty<bool> ControlButtonsVisibleProperty =
        AvaloniaProperty.Register<LayerControl, bool>(nameof(ControlButtonsVisible), false);

    public bool ControlButtonsVisible
    {
        get => GetValue(ControlButtonsVisibleProperty);
        set => SetValue(ControlButtonsVisibleProperty, value);
    }

    public string LayerColor
    {
        get => GetValue(LayerColorProperty);
        set => SetValue(LayerColorProperty, value);
    }

    public static readonly StyledProperty<string> LayerColorProperty =
        AvaloniaProperty.Register<LayerControl, string>(nameof(LayerColor), "#00000000");

    public static readonly StyledProperty<LayersManager> ManagerProperty =
        AvaloniaProperty.Register<LayerControl, LayersManager>(nameof(Manager));

    public LayersManager Manager
    {
        get => GetValue(ManagerProperty);
        set => SetValue(ManagerProperty, value);
    }

    public static readonly StyledProperty<RelayCommand> MoveToBackCommandProperty =
        AvaloniaProperty.Register<LayerControl, RelayCommand>(nameof(MoveToBackCommand));

    public RelayCommand MoveToBackCommand
    {
        get => GetValue(MoveToBackCommandProperty);
        set => SetValue(MoveToBackCommandProperty, value);
    }

    public static readonly StyledProperty<RelayCommand> MoveToFrontCommandProperty =
        AvaloniaProperty.Register<LayerControl, RelayCommand>(nameof(MoveToFrontCommand));

    public RelayCommand MoveToFrontCommand
    {
        get => GetValue(MoveToFrontCommandProperty);
        set => SetValue(MoveToFrontCommandProperty, value);
    }


    private MouseUpdateController mouseUpdateController;

    public LayerControl()
    {
        InitializeComponent();
        Loaded += LayerControl_Loaded;

        if (App.Current.TryGetResource("SoftSelectedLayerBrush", App.Current.ActualThemeVariant, out var value))
        {
            highlightColor = value as IBrush;
        }
    }

    private void LayerControl_Loaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, Manager.LayerControl_MouseMove);
    }

    public static void RemoveDragEffect(Grid grid)
    {
        grid.Background = Brushes.Transparent;
    }

    private void LayerItem_OnMouseEnter(object sender, PointerEventArgs e)
    {
        ControlButtonsVisible = true;
    }

    private void LayerItem_OnMouseLeave(object sender, PointerEventArgs e)
    {
        ControlButtonsVisible = false;
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
        object droppedLayer = droppedMemberDataObject.Get(LayerControlDataName);
        object droppedFolder = droppedMemberDataObject.Get(AvaloniaUI.Views.Layers.FolderControl.FolderControlDataName);
        if (droppedLayer is LayerControl layer)
            return layer.Layer.GuidValue;
        else if (droppedFolder is AvaloniaUI.Views.Layers.FolderControl folder)
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

    private void MaskMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (Layer is not null)
            Layer.ShouldDrawOnMask = true;
    }

    private void LayerMouseDown(object sender, PointerPressedEventArgs e)
    {
        if (Layer is not null)
            Layer.ShouldDrawOnMask = false;
    }
}
