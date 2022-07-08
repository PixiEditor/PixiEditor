using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;
#nullable enable
internal partial class LayersManager : UserControl
{
    public static readonly DependencyProperty ActiveDocumentProperty =
        DependencyProperty.Register(nameof(ActiveDocument), typeof(DocumentViewModel), typeof(LayersManager), new(null));

    public DocumentViewModel? ActiveDocument
    {
        get => (DocumentViewModel)GetValue(ActiveDocumentProperty);
        set => SetValue(ActiveDocumentProperty, value);
    }

    private readonly Brush? highlightColor;
    public LayersManager()
    {
        InitializeComponent();
        numberInput.OnScrollAction = () => NumberInput_LostFocus(null, null);
        highlightColor = (Brush?)App.Current.Resources["SoftSelectedLayerColor"];
    }

    private void LayerStructureItemContainer_MouseMove(object? sender, System.Windows.Input.MouseEventArgs? e)
    {
        if (e is null)
            return;
        if (sender is LayerControl container
            && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed /*&& !container.Layer.IsRenaming*/)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
    }

    private void LayerGroup_MouseMove(object? sender, System.Windows.Input.MouseEventArgs? e)
    {
        if (e is null)
            return;
        if (sender is FolderControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed
                                                  /*&& !container.GroupData.IsRenaming*/)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
    }

    private void NumberInput_LostFocus(object? sender, RoutedEventArgs? e)
    {
        if (ActiveDocument?.SelectedStructureMember is null)
            return;
        ActiveDocument.SetMemberOpacity(ActiveDocument.SelectedStructureMember.GuidValue, numberInput.Value / 100f);

        // does anyone know why this is here?
        ShortcutController.UnblockShortcutExecutionAll();
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        dropBorder.BorderBrush = Brushes.Transparent;
        Guid? droppedGuid = LayerControl.ExtractMemberGuid(e.Data);
        if (droppedGuid is null || ActiveDocument is null)
            return;
        ActiveDocument.MoveStructureMember((Guid)droppedGuid, ActiveDocument.StructureRoot.Children[0].GuidValue, StructureMemberPlacement.Below);
    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        ((Border)sender).BorderBrush = highlightColor;
    }

    private void Grid_DragLeave(object sender, DragEventArgs e)
    {
        ((Border)sender).BorderBrush = Brushes.Transparent;
    }
}
