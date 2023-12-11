using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Layers;
#nullable enable
internal partial class FolderControl : UserControl
{

    public static readonly StyledProperty<FolderViewModel> FolderProperty =
        AvaloniaProperty.Register<FolderControl, FolderViewModel>(nameof(Folder));

    public FolderViewModel Folder
    {
        get => GetValue(FolderProperty);
        set => SetValue(FolderProperty, value);
    }

    public static string? FolderControlDataName = typeof(FolderControl).FullName;

    public static readonly StyledProperty<LayersManager> ManagerProperty =
        AvaloniaProperty.Register<FolderControl, LayersManager>(nameof(Manager));

    public LayersManager Manager
    {
        get { return GetValue(ManagerProperty); }
        set { SetValue(ManagerProperty, value); }
    }

    private readonly IBrush? highlightColor;

    
    private MouseUpdateController mouseUpdateController;

    public FolderControl()
    {
        InitializeComponent();
        highlightColor = (Brush?)App.Current.Resources["SoftSelectedLayerColor"];
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, Manager.FolderControl_MouseMove);
    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        Grid item = (Grid)sender;
        item.Background = highlightColor;
    }

    private void Grid_CenterEnter(object sender, DragEventArgs e)
    {
        centerGrid.Background = highlightColor;
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

    private void HandleDrop(IDataObject dataObj, StructureMemberPlacement placement)
    {
        Guid? droppedMemberGuid = LayerControl.ExtractMemberGuid(dataObj);
        if (droppedMemberGuid is null)
            return;
        Folder.Document.Operations.MoveStructureMember((Guid)droppedMemberGuid, Folder.GuidValue, placement);
    }

    private void Grid_Drop_Top(object sender, DragEventArgs e)
    {
        LayerControl.RemoveDragEffect((Grid)sender);
        HandleDrop(e.Data, StructureMemberPlacement.Above);
    }

    private void Grid_Drop_Center(object sender, DragEventArgs e)
    {
        LayerControl.RemoveDragEffect(centerGrid);
        HandleDrop(e.Data, StructureMemberPlacement.Inside);
    }

    private void Grid_Drop_Bottom(object sender, DragEventArgs e)
    {
        LayerControl.RemoveDragEffect((Grid)sender);
        HandleDrop(e.Data, StructureMemberPlacement.Below);
    }

    private void FolderControl_DragEnter(object sender, DragEventArgs e)
    {
        middleDropGrid.IsVisible = true;
    }

    private void FolderControl_DragLeave(object sender, DragEventArgs e)
    {
        middleDropGrid.IsVisible = false;
    }

    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        editableTextBlock.EnableEditing();
    }
}
