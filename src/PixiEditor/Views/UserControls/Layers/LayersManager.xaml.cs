using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.ChangeableDocument.Enums;
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

    private void LayerControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        LayerControl control = (LayerControl)sender;
        if (e.ChangedButton == MouseButton.Left)
        {
            HandleMouseDown(control.Layer);
            control.CaptureMouse();
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
    
    public void LayerControl_MouseMove(object? sender, System.Windows.Input.MouseEventArgs? e)
    {
        if (e is null)
            return;
        if (sender is LayerControl container &&
            e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
            container.IsMouseCaptured)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
    }

    private void LayerControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not LayerControl layerControl)
            return;
        layerControl.ReleaseMouseCapture();
    }
    
    private void FolderControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        FolderControl control = (FolderControl)sender;
        if (e.ChangedButton == MouseButton.Left)
        {
            HandleMouseDown(control.Folder);
            control.CaptureMouse();
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

    public void FolderControl_MouseMove(object? sender, System.Windows.Input.MouseEventArgs? e)
    {
        if (e is null)
            return;
        if (sender is FolderControl container && 
            e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
            container.IsMouseCaptured)
        {
            Dispatcher.InvokeAsync(() => DragDrop.DoDragDrop(container, container, DragDropEffects.Move));
        }
    }
    
    private void FolderControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FolderControl folderControl)
            return;
        folderControl.ReleaseMouseCapture();
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

            ViewModelMain.Current.ActionDisplays[nameof(LayersManager)] = "Import as new layer";
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.Move;
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

    private void HandleMouseDown(StructureMemberViewModel memberVM)
    {
        if (ActiveDocument is null)
            return;
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            if (memberVM.Selection == StructureMemberSelectionType.Hard)
                return;
            else if (memberVM.Selection == StructureMemberSelectionType.Soft)
                ActiveDocument.Operations.RemoveSoftSelectedMember(memberVM.GuidValue);
            else if (memberVM.Selection == StructureMemberSelectionType.None)
                ActiveDocument.Operations.AddSoftSelectedMember(memberVM.GuidValue);
        }
        else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
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
