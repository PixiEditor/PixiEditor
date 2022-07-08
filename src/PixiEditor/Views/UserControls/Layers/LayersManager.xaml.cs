using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls.Layers;

internal partial class LayersManager : UserControl
{
    public static readonly DependencyProperty ActiveDocumentProperty =
        DependencyProperty.Register(nameof(ActiveDocument), typeof(DocumentViewModel), typeof(LayersManager), new(null));

    public DocumentViewModel ActiveDocument
    {
        get => (DocumentViewModel)GetValue(ActiveDocumentProperty);
        set => SetValue(ActiveDocumentProperty, value);
    }

    public LayersManager()
    {
        InitializeComponent();
        numberInput.OnScrollAction = () => NumberInput_LostFocus(null, null);
    }

    private void LayerStructureItemContainer_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        /*
        if (sender is LayerStructureItemContainer container
            && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && !container.Layer.IsRenaming)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
        */
    }

    private void LayerGroup_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        /*
        if (sender is LayerGroupControl container && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed
                                                  && !container.GroupData.IsRenaming)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
        */
    }

    private void NumberInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (ActiveDocument?.SelectedStructureMember is null)
            return;
        ActiveDocument.SetMemberOpacity(ActiveDocument.SelectedStructureMember.GuidValue, numberInput.Value / 100f);

        // does anyone know why this is here?
        ShortcutController.UnblockShortcutExecutionAll();
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        /*
        dropBorder.BorderBrush = Brushes.Transparent;

        if (e.Data.GetDataPresent(LayerGroupControl.LayerContainerDataName))
        {
            HandleLayerDrop(e.Data);
        }

        if (e.Data.GetDataPresent(LayerGroupControl.LayerGroupControlDataName))
        {
            HandleGroupControlDrop(e.Data);
        }
        */
    }
    /*
    private void HandleLayerDrop(IDataObject data)
    {
        var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
        if (doc.Layers.Count == 0) return;

        var layerContainer = (LayerStructureItemContainer)data.GetData(LayerGroupControl.LayerContainerDataName);
        var refLayer = doc.Layers[0].GuidValue;
        doc.MoveLayerInStructure(layerContainer.Layer.GuidValue, refLayer);
        doc.LayerStructure.AssignParent(layerContainer.Layer.GuidValue, null);
    }

    private void HandleGroupControlDrop(IDataObject data)
    {
        var doc = LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument;
        var groupContainer = (LayerGroupControl)data.GetData(LayerGroupControl.LayerGroupControlDataName);
        doc.LayerStructure.MoveGroup(groupContainer.GroupGuid, 0);
    }*/

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        ((Border)sender).BorderBrush = LayerControl.HighlightColor;
    }

    private void Grid_DragLeave(object sender, DragEventArgs e)
    {
        ((Border)sender).BorderBrush = Brushes.Transparent;
    }
}
