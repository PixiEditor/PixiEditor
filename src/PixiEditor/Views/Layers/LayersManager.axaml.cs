using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Helpers;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Views.Layers;
#nullable enable
internal partial class LayersManager : UserControl
{
    public const string LayersDataName = "PixiEditor.LayersData";
    public DocumentViewModel ActiveDocument => DataContext is LayersDockViewModel vm ? vm.ActiveDocument : null;
    private readonly IBrush? highlightColor;

    public LayersManager()
    {
        InitializeComponent();
        numberInput.OnScrollAction = () => NumberInput_LostFocus(null, null);
        if (App.Current.TryGetResource("SoftSelectedLayerBrush", App.Current.ActualThemeVariant, out var value))
        {
            highlightColor = value as IBrush;
        }

        dropBorder.AddHandler(DragDrop.DragEnterEvent, Grid_DragEnter);
        dropBorder.AddHandler(DragDrop.DragLeaveEvent, Grid_DragLeave);
        dropBorder.AddHandler(DragDrop.DropEvent, Grid_Drop);
        RootGrid.AddHandler(DragDrop.DropEvent, Grid_Drop);
        treeView.AddHandler(DragDrop.DragOverEvent, TreeView_DragScroll);
    }

    private void LayerControl_MouseDown(object sender, PointerPressedEventArgs e)
    {
        LayerControl control = (LayerControl)sender;
        if (e.GetMouseButton(this) == MouseButton.Left)
        {
            HandleMouseDown(control.Layer, e);
            e.Pointer.Capture(control);
        }
        else
        {
            if (control.Layer is not null && control.Layer.Selection == StructureMemberSelectionType.None)
            {
                control.Layer.Document.Operations.SetSelectedMember(control.Layer.Id);
                control.Layer.Document.Operations.ClearSoftSelectedMembers();
            }
        }
    }

    public void LayerControl_MouseMove(PointerEventArgs e)
    {
        if (e is null)
            return;

        bool isLeftPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

        if (e.Source is LayerControl container && isLeftPressed && Equals(e.Pointer.Captured, container))
        {
            DataObject data = new();
            Guid[] selectedGuids = container.Layer.Document.GetSelectedMembersInOrder().ToArray();
            data.Set(LayersDataName, selectedGuids);
            Dispatcher.UIThread.InvokeAsync(() => DragDrop.DoDragDrop(e, data, DragDropEffects.Move));
        }
    }

    private void LayerControl_MouseUp(object sender, PointerReleasedEventArgs e)
    {
        if (sender is not LayerControl)
            return;

        if (e is { Source: LayerControl layerControl, InitialPressMouseButton: MouseButton.Left } &&
            !e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (layerControl.Layer is not null)
            {
                layerControl.Layer.Document.Operations.SetSelectedMember(layerControl.Layer.Id);
                layerControl.Layer.Document.Operations.ClearSoftSelectedMembers();
            }
        }

        e.Pointer.Capture(null);
    }

    private void FolderControl_MouseDown(object sender, PointerPressedEventArgs e)
    {
        FolderControl control = (FolderControl)sender;
        if (e.GetMouseButton(control) == MouseButton.Left)
        {
            HandleMouseDown(control.Folder, e);
            e.Pointer.Capture(control);
        }
        else
        {
            if (control.Folder is not null && control.Folder.Selection == StructureMemberSelectionType.None)
            {
                control.Folder.Document.Operations.SetSelectedMember(control.Folder.Id);
                control.Folder.Document.Operations.ClearSoftSelectedMembers();
            }
        }
    }

    public void FolderControl_MouseMove(PointerEventArgs e)
    {
        if (e is null)
            return;

        bool isLeftPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        if (e.Source is FolderControl container &&
            isLeftPressed && Equals(e.Pointer.Captured, container))
        {
            DataObject data = new();
            Guid[] selectedGuids = container.Folder.Document.GetSelectedMembersInOrder().ToArray();
            data.Set(LayersDataName, selectedGuids);
            Dispatcher.UIThread.InvokeAsync(() => DragDrop.DoDragDrop(e, data, DragDropEffects.Move));
        }
    }

    private void FolderControl_MouseUp(object sender, PointerReleasedEventArgs e)
    {
        if (sender is not FolderControl folderControl)
            return;

        if (e is { Source: FolderControl layerControl, InitialPressMouseButton: MouseButton.Left } &&
            !e.KeyModifiers.HasFlag(KeyModifiers.Control) && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (layerControl.Folder is not null)
            {
                layerControl.Folder.Document.Operations.SetSelectedMember(layerControl.Folder.Id);
                layerControl.Folder.Document.Operations.ClearSoftSelectedMembers();
            }
        }

        e.Pointer.Capture(null);
    }

    private void NumberInput_LostFocus(object? sender, RoutedEventArgs? e)
    {
        if (ActiveDocument?.SelectedStructureMember is null)
            return;
        //ActiveDocument.SetMemberOpacity(ActiveDocument.SelectedStructureMember.GuidValue, numberInput.Value / 100f);

        // does anyone know why this is here?
        ShortcutController.UnblockShortcutExecutionAll();
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(LayersManager)] = null;

        if (ActiveDocument == null)
        {
            return;
        }

        dropBorder.BorderBrush = Brushes.Transparent;
        Guid[]? droppedGuids = LayerControl.ExtractMemberGuids(e.Data);
        if (droppedGuids != null)
        {
            using var block = ActiveDocument.Operations.StartChangeBlock();
            Guid lastMovedMember = ActiveDocument.NodeGraph.StructureTree.Members[^1].Id;

            foreach (Guid memberGuid in droppedGuids)
            {
                ActiveDocument.Operations.MoveStructureMember(memberGuid, lastMovedMember,
                    StructureMemberPlacement.Below);
                lastMovedMember = memberGuid;
            }

            e.Handled = true;
        }

        if (ClipboardController.TryPaste(ActiveDocument, new[] { (IDataObject)e.Data }, true))
        {
            e.Handled = true;
        }
    }

    private void Grid_DragEnter(object sender, DragEventArgs e)
    {
        if (ActiveDocument == null)
        {
            return;
        }

        var member = LayerControl.ExtractMemberGuids(e.Data);

        if (member == null)
        {
            if (!ClipboardController.IsImage((IDataObject)e.Data))
            {
                return;
            }

            ViewModelMain.Current.ActionDisplays[nameof(LayersManager)] = "IMPORT_AS_NEW_LAYER";
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.Move;
        }

        ((Border)sender).BorderBrush = highlightColor;
        e.Handled = true;
    }

    private void Grid_DragLeave(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(LayersManager)] = null;
        ((Border)sender).BorderBrush = Brushes.Transparent;
    }

    private void HandleMouseDown(IStructureMemberHandler memberVM, PointerPressedEventArgs pointerPressedEventArgs)
    {
        if (ActiveDocument is null)
            return;

        if (pointerPressedEventArgs.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (memberVM.Selection == StructureMemberSelectionType.Hard)
                return;
            else if (memberVM.Selection == StructureMemberSelectionType.Soft)
                ActiveDocument.Operations.RemoveSoftSelectedMember(memberVM.Id);
            else if (memberVM.Selection == StructureMemberSelectionType.None)
                ActiveDocument.Operations.AddSoftSelectedMember(memberVM.Id);
        }
        else if (pointerPressedEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (ActiveDocument.SelectedStructureMember is null ||
                ActiveDocument.SelectedStructureMember.Id == memberVM.Id)
                return;
            ActiveDocument.Operations.ClearSoftSelectedMembers();

            TraverseRange(
                ActiveDocument.SelectedStructureMember.Id,
                memberVM.Id,
                ActiveDocument.NodeGraph.StructureTree.Members,
                static member =>
                {
                    if (member.Selection == StructureMemberSelectionType.None)
                        member.Document.Operations.AddSoftSelectedMember(member.Id);
                });
        }
        else if (!ActiveDocument.SelectedMembers.Contains(memberVM.Id))
        {
            ActiveDocument.Operations.SetSelectedMember(memberVM.Id);
            ActiveDocument.Operations.ClearSoftSelectedMembers();
        }
    }

    private static int TraverseRange(Guid bound1, Guid bound2, IEnumerable<IStructureMemberHandler> root,
        Action<IStructureMemberHandler> action, int matches = 0)
    {
        if (matches == 2)
            return 2;

        var reversed = root.Reverse();
        foreach (var child in reversed)
        {
            if (child is FolderNodeViewModel innerFolder)
            {
                matches = TraverseRange(bound1, bound2, innerFolder.Children, action, matches);
            }

            if (matches == 1)
                action(child);
            if (matches == 2)
                return 2;
            if (child.Id == bound1 || child.Id == bound2)
            {
                matches++;
                if (matches == 1)
                    action(child);
                if (matches == 2)
                    return 2;
            }
        }

        return matches;
    }

    private void TreeView_DragScroll(object sender, DragEventArgs e)
    {
        if (sender is not TreeView treeView)
            return;

        var point = e.GetPosition(treeView);

        ScrollViewer scrollViewer = treeView.FindDescendantOfType<ScrollViewer>();

        if (scrollViewer is null)
            return;

        if (point.Y < 10)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y - 10);
        }
        else if (point.Y > treeView.Bounds.Height - 10)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y + 10);
        }
    }
}
