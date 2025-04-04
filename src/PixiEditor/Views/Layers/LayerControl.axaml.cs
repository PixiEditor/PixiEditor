using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Views.Layers;
#nullable enable
internal partial class LayerControl : UserControl
{
    public static readonly StyledProperty<ILayerHandler> LayerProperty =
        AvaloniaProperty.Register<LayerControl, ILayerHandler>(nameof(Layer));

    public ILayerHandler Layer
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
        Unloaded += LayerControl_Unloaded;

        if (App.Current.TryGetResource("SoftSelectedLayerBrush", App.Current.ActualThemeVariant, out var value))
        {
            highlightColor = value as IBrush;
        }

        TopGrid.AddHandler(DragDrop.DragEnterEvent, Grid_DragEnter);
        TopGrid.AddHandler(DragDrop.DragLeaveEvent, Grid_DragLeave);
        TopGrid.AddHandler(DragDrop.DropEvent, Grid_Drop_Top);
        dropBelowGrid.AddHandler(DragDrop.DragEnterEvent, Grid_DragEnter);
        dropBelowGrid.AddHandler(DragDrop.DragLeaveEvent, Grid_DragLeave);
        dropBelowGrid.AddHandler(DragDrop.DropEvent, Grid_Drop_Below);
        thirdDropGrid.AddHandler(DragDrop.DragEnterEvent, Grid_DragEnter);
        thirdDropGrid.AddHandler(DragDrop.DragLeaveEvent, Grid_DragLeave);
        thirdDropGrid.AddHandler(DragDrop.DropEvent, Grid_Drop_Bottom);
    }

    private void LayerControl_Unloaded(object? sender, RoutedEventArgs e)
    {
        mouseUpdateController?.Dispose();
    }

    private void LayerControl_Loaded(object? sender, RoutedEventArgs e)
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

    public static Guid[]? ExtractMemberGuids(IDataObject droppedMemberDataObject)
    {
        object droppedLayer = droppedMemberDataObject.Get(LayersManager.LayersDataName);
        if (droppedLayer is null)
            return null;

        if (droppedLayer is Guid droppedLayerGuid)
            return new[] { droppedLayerGuid };

        if (droppedLayer is Guid[] droppedLayerGuids)
        {
            return droppedLayerGuids;
        }

        return null;
    }

    private bool HandleDrop(IDataObject dataObj, StructureMemberPlacement placement)
    {
        if (placement == StructureMemberPlacement.Inside)
            return false;
        Guid[]? droppedMemberGuids = ExtractMemberGuids(dataObj);
        if (droppedMemberGuids is null)
            return false;
        if (Layer is null)
            return false;

        if(placement is StructureMemberPlacement.Below or StructureMemberPlacement.BelowOutsideFolder)
        {
            droppedMemberGuids = droppedMemberGuids.Reverse().ToArray();
        }

        var document = Layer.Document;

        using var block = Layer.Document.Operations.StartChangeBlock();
        Guid lastMovedMember = Layer.Id;
        foreach (Guid memberGuid in droppedMemberGuids)
        {
            document.Operations.MoveStructureMember(memberGuid, lastMovedMember, placement);
            lastMovedMember = memberGuid;
            block.ExecuteQueuedActions();
        }

        return true;
    }

    private void Grid_Drop_Top(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        e.Handled = HandleDrop(e.Data, StructureMemberPlacement.Above);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        e.Handled = HandleDrop(e.Data, StructureMemberPlacement.Below);
    }

    private void Grid_Drop_Below(object sender, DragEventArgs e)
    {
        RemoveDragEffect((Grid)sender);
        e.Handled = HandleDrop(e.Data, StructureMemberPlacement.BelowOutsideFolder);
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
