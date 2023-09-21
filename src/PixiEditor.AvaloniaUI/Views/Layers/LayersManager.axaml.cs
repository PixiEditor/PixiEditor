using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Hardware.Info;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Dock;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Layers;
#nullable enable
internal partial class LayersManager : UserControl
{
    public DocumentViewModel ActiveDocument => DataContext is LayersDockViewModel vm ? vm.ActiveDocument : null;
    private readonly IBrush? highlightColor;
    public LayersManager()
    {
        InitializeComponent();
        numberInput.OnScrollAction = () => NumberInput_LostFocus(null, null);
        highlightColor = (Brush?)App.Current.Resources["SoftSelectedLayerColor"];
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
                control.Layer.Document.Operations.SetSelectedMember(control.Layer.GuidValue);
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
            //TODO: Check what dataformat to use
            data.Set(DataFormats.Text, container);
            Dispatcher.UIThread.InvokeAsync(() => DragDrop.DoDragDrop(e, data, DragDropEffects.Move));
        }
    }

    private void LayerControl_MouseUp(object sender, PointerReleasedEventArgs e)
    {
        if (sender is not LayerControl)
            return;

        e.Pointer.Capture(null);
    }
    
    private void FolderControl_MouseDown(object sender, PointerPressedEventArgs e)
    {
        AvaloniaUI.Views.Layers.FolderControl control = (AvaloniaUI.Views.Layers.FolderControl)sender;
        if (e.GetMouseButton(control) == MouseButton.Left)
        {
            HandleMouseDown(control.Folder, e);
            e.Pointer.Capture(control);
        }
        else
        {
            if (control.Folder is not null && control.Folder.Selection == StructureMemberSelectionType.None)
            {
                control.Folder.Document.Operations.SetSelectedMember(control.Folder.GuidValue);
                control.Folder.Document.Operations.ClearSoftSelectedMembers();
            }
        }
    }

    public void FolderControl_MouseMove(PointerEventArgs e)
    {
        if (e is null)
            return;

        bool isLeftPressed = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;
        if (e.Source is AvaloniaUI.Views.Layers.FolderControl container &&
            isLeftPressed && Equals(e.Pointer.Captured, container))
        {
            DataObject data = new();
            data.Set(DataFormats.Text, container);
            Dispatcher.UIThread.InvokeAsync(() => DragDrop.DoDragDrop(e, data, DragDropEffects.Move));
        }
    }
    
    private void FolderControl_MouseUp(object sender, PointerReleasedEventArgs e)
    {
        if (sender is not AvaloniaUI.Views.Layers.FolderControl folderControl)
            return;

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
        Guid? droppedGuid = LayerControl.ExtractMemberGuid(e.Data);

        if (droppedGuid is not null && ActiveDocument is not null)
        {
            ActiveDocument.Operations.MoveStructureMember((Guid)droppedGuid,
                ActiveDocument.StructureRoot.Children[0].GuidValue, StructureMemberPlacement.Below);
            e.Handled = true;
        }

        if (ClipboardController.TryPaste(ActiveDocument, (DataObject)e.Data, true))
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
        
        var member = LayerControl.ExtractMemberGuid(e.Data);

        if (member == null)
        {
            if (!ClipboardController.IsImage((DataObject)e.Data))
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

    private static int TraverseRange(Guid bound1, Guid bound2, FolderViewModel root, Action<StructureMemberViewModel> action, int matches = 0)
    {
        if (matches == 2)
            return 2;
        foreach (StructureMemberViewModel child in root.Children)
        {
            if (child is FolderViewModel innerFolder)
            {
                matches = TraverseRange(bound1, bound2, innerFolder, action, matches);
            }
            if (matches == 1)
                action(child);
            if (matches == 2)
                return 2;
            if (child.GuidValue == bound1 || child.GuidValue == bound2)
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

    private void HandleMouseDown(StructureMemberViewModel memberVM, PointerPressedEventArgs pointerPressedEventArgs)
    {
        if (ActiveDocument is null)
            return;

        if (pointerPressedEventArgs.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (memberVM.Selection == StructureMemberSelectionType.Hard)
                return;
            else if (memberVM.Selection == StructureMemberSelectionType.Soft)
                ActiveDocument.Operations.RemoveSoftSelectedMember(memberVM.GuidValue);
            else if (memberVM.Selection == StructureMemberSelectionType.None)
                ActiveDocument.Operations.AddSoftSelectedMember(memberVM.GuidValue);
        }
        else if (pointerPressedEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (ActiveDocument.SelectedStructureMember is null || ActiveDocument.SelectedStructureMember.GuidValue == memberVM.GuidValue)
                return;
            ActiveDocument.Operations.ClearSoftSelectedMembers();
            TraverseRange(
                ActiveDocument.SelectedStructureMember.GuidValue,
                memberVM.GuidValue,
                ActiveDocument.StructureRoot,
                static member =>
                {
                    if (member.Selection == StructureMemberSelectionType.None)
                        member.Document.Operations.AddSoftSelectedMember(member.GuidValue);
                });
        }
        else
        {
            ActiveDocument.Operations.SetSelectedMember(memberVM.GuidValue);
            ActiveDocument.Operations.ClearSoftSelectedMembers();
        }
    }
}
