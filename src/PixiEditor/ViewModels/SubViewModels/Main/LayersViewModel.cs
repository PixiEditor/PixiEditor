using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Layer", "Image")]
internal class LayersViewModel : SubViewModel<ViewModelMain>
{

    public LayersViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    public void CreateFolderFromActiveLayers(object parameter)
    {

    }

    public bool CanCreateFolderFromSelected(object obj)
    {
        return false;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDeleteSelected")]
    public bool CanDeleteSelected(object parameter)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return false;
        return true;
    }

    [Command.Basic("PixiEditor.Layer.DeleteSelected", "Delete active layer/folder", "Delete active layer or folder", CanExecute = "PixiEditor.Layer.CanDeleteSelected")]
    public void DeleteSelected(object parameter)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return;
        member.Document.DeleteStructureMember(member.GuidValue);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDeleteAllSelected")]
    public bool CanDeleteAllSelected(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.SelectedStructureMember is not null || doc.SoftSelectedStructureMembers.Count > 0;
    }

    [Command.Basic("PixiEditor.Layer.DeleteAllSelected", "Delete all selected layers/folders", "Delete all selected layers and/or folders", CanExecute = "PixiEditor.Layer.CanDeleteAllSelected")]
    public void DeleteAllSelected(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        List<Guid> membersToDelete = new();
        if (doc.SelectedStructureMember is not null)
            membersToDelete.Add(doc.SelectedStructureMember.GuidValue);
        membersToDelete.AddRange(doc.SoftSelectedStructureMembers.Select(static member => member.GuidValue));
        doc.DeleteStructureMembers(membersToDelete);
    }

    [Command.Basic("PixiEditor.Layer.NewFolder", "New Folder", "Create new folder", CanExecute = "PixiEditor.Layer.CanCreateNewMember")]
    public void NewFolder(object parameter)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.CreateStructureMember(StructureMemberType.Folder);
    }

    [Command.Basic("PixiEditor.Layer.NewLayer", "New Layer", "Create new layer", CanExecute = "PixiEditor.Layer.CanCreateNewMember", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
    public void NewLayer(object parameter)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.CreateStructureMember(StructureMemberType.Layer);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanCreateNewMember")]
    public bool CanCreateNewMember(object parameter)
    {
        return Owner.DocumentManagerSubViewModel.ActiveDocument is not null;
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragStarted")]
    public void OpacitySliderDragStarted(object parameter)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.UseOpacitySlider();
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragStarted();
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragged")]
    public void OpacitySliderDragged(object parameter)
    {
        if (parameter is not double value)
            return;
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragged((float)value);
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragEnded")]
    public void OpacitySliderDragEnded(object parameter)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.OnOpacitySliderDragEnded();
    }

    public void DuplicateLayer(object parameter)
    {

    }

    public bool CanDuplicateLayer(object property)
    {
        return false;
    }

    public void RenameLayer(object parameter)
    {

    }

    public bool CanRenameLayer(object parameter)
    {
        return false;
    }

    private bool CanMoveSelectedMember(bool upwards)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return false;
        var path = doc!.StructureViewModel.FindPath(member.GuidValue);
        if (path.Count < 2)
            return false;
        var parent = (FolderViewModel)path[1];
        int index = parent.Children.IndexOf(path[0]);
        if (upwards && index == parent.Children.Count - 1)
            return false;
        if (!upwards && index == 0)
            return false;
        return true;
    }

    private void MoveSelectedMember(bool upwards)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return;
        var path = doc!.StructureViewModel.FindPath(member.GuidValue);
        if (path.Count < 2)
            return;
        var parent = (FolderViewModel)path[1];
        int curIndex = parent.Children.IndexOf(path[0]);
        if (upwards)
        {
            if (curIndex == parent.Children.Count - 1)
                return;
            doc.MoveStructureMember(member.GuidValue, parent.Children[curIndex + 1].GuidValue, StructureMemberPlacement.Above);
        }
        else
        {
            if (curIndex == 0)
                return;
            doc.MoveStructureMember(member.GuidValue, parent.Children[curIndex - 1].GuidValue, StructureMemberPlacement.Below);
        }
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanMoveSelectedMemberUpwards")]
    public bool CanMoveSelectedMemberUpwards(object property) => CanMoveSelectedMember(true);
    [Evaluator.CanExecute("PixiEditor.Layer.CanMoveSelectedMemberDownwards")]
    public bool CanMoveSelectedMemberDownwards(object property) => CanMoveSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberUpwards", "Move selected layer or folder upwards", "Move selected layer or folder upwards", CanExecute = "PixiEditor.Layer.CanMoveSelectedMemberUpwards")]
    public void MoveSelectedMemberUpwards(object parameter) => MoveSelectedMember(true);
    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberDownwards", "Move selected layer or folder downwards", "Move selected layer or folder downwards", CanExecute = "PixiEditor.Layer.CanMoveSelectedMemberDownwards")]
    public void MoveSelectedMemberDownwards(object parameter) => MoveSelectedMember(false);

    public bool CanMergeSelected(object obj)
    {
        return false;
    }

    public void MergeSelected(object parameter)
    {

    }

    public bool CanMergeWithAbove(object property)
    {
        return false;
    }

    public void MergeWithAbove(object parameter)
    {

    }

    public bool CanMergeWithBelow(object property)
    {
        return false;
    }

    public void MergeWithBelow(object parameter)
    {

    }
}
