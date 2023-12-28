using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;
#nullable enable
internal partial class FolderControl : UserControl
{
    public static readonly DependencyProperty FolderProperty =
        DependencyProperty.Register(nameof(Folder), typeof(FolderViewModel), typeof(FolderControl), new(null));

    public FolderViewModel Folder
    {
        get => (FolderViewModel)GetValue(FolderProperty);
        set => SetValue(FolderProperty, value);
    }

    public static string? FolderControlDataName = typeof(FolderControl).FullName;
    public static string? LayerControlDataName = typeof(LayerControl).FullName;

    public static readonly DependencyProperty ManagerProperty = DependencyProperty.Register(
        nameof(Manager), typeof(LayersManager), typeof(FolderControl), new PropertyMetadata(default(LayersManager)));

    public LayersManager Manager
    {
        get { return (LayersManager)GetValue(ManagerProperty); }
        set { SetValue(ManagerProperty, value); }
    }

    private readonly Brush? highlightColor;
    
    private MouseUpdateController? mouseUpdateController;

    public FolderControl()
    {
        InitializeComponent();
        highlightColor = (Brush?)App.Current.Resources["SoftSelectedLayerColor"];
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController?.Dispose();
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
        middleDropGrid.Visibility = Visibility.Visible;
    }

    private void FolderControl_DragLeave(object sender, DragEventArgs e)
    {
        middleDropGrid.Visibility = Visibility.Collapsed;
    }

    private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
    {
        editableTextBlock.EnableEditing();
    }
}
