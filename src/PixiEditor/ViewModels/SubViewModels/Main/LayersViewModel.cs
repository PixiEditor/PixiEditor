using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
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

    [Evaluator.CanExecute("PixiEditor.Layer.HasSelectedMembers")]
    public bool HasSelectedMembers(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.SelectedStructureMember is not null || doc.SoftSelectedStructureMembers.Count > 0;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasMultipleSelectedMembers")]
    public bool HasMultipleSelectedMembers(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        int count = doc.SoftSelectedStructureMembers.Count;
        if (doc.SelectedStructureMember is not null)
            count++;
        return count > 1;
    }

    private List<Guid> GetSelected()
    {
        List<Guid> membersToDelete = new();
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return membersToDelete;
        if (doc.SelectedStructureMember is not null)
            membersToDelete.Add(doc.SelectedStructureMember.GuidValue);
        membersToDelete.AddRange(doc.SoftSelectedStructureMembers.Select(static member => member.GuidValue));
        return membersToDelete;
    }

    [Command.Basic("PixiEditor.Layer.DeleteAllSelected", "Delete all selected layers/folders", "Delete all selected layers and/or folders", CanExecute = "PixiEditor.Layer.HasSelectedMembers")]
    public void DeleteAllSelected(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = GetSelected();
        if (selected.Count > 0)
            doc.DeleteStructureMembers(selected);
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
        return Owner.DocumentManagerSubViewModel.ActiveDocument is { } doc && !doc.UpdateableChangeActive;
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

    [Command.Basic("PixiEditor.Layer.DuplicateSelectedLayer", "Duplicate selected layer", "Duplicate selected layer", CanExecute = "PixiEditor.Layer.CanDuplicatedSelectedLayer")]
    public void DuplicateLayer(object parameter)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is not LayerViewModel layerVM)
            return;
        member.Document.DuplicateLayer(member.GuidValue);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDuplicatedSelectedLayer")]
    public bool CanDuplicateLayer(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is LayerViewModel;
    }

    private bool HasSelectedMember(bool above)
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
        if (above && index == parent.Children.Count - 1)
            return false;
        if (!above && index == 0)
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

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasMask")]
    public bool ActiveMemberHasMask() => Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false;

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasNoMask")]
    public bool ActiveLayerHasNoMask() => !ActiveMemberHasMask();

    private bool? DoesMaskVisibilityEqual(bool value)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return false;
        return member.MaskIsVisibleBindable == value;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanEnableMask")]
    public bool CanEnableMask() => DoesMaskVisibilityEqual(false) ?? false;

    [Evaluator.CanExecute("PixiEditor.Layer.CanDisableMask")]
    public bool CanDisableMask() => DoesMaskVisibilityEqual(true) ?? false;

    [Command.Basic("PixiEditor.Layer.EnableMask", "Enable mask", "Enable mask", CanExecute = "PixiEditor.Layer.CanEnableMask", Parameter = true)]
    [Command.Basic("PixiEditor.Layer.DisableMask", "Disable mask", "Disable mask", CanExecute = "PixiEditor.Layer.CanDisableMask", Parameter = false)]
    public void ToggleMaskVisibility(bool newVisibility)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable || member.MaskIsVisibleBindable == newVisibility)
            return;
        member.MaskIsVisibleBindable = newVisibility;
    }

    [Command.Basic("PixiEditor.Layer.CreateMask", "Create mask", "Create mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasNoMask")]
    public void CreateMask(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || member.HasMaskBindable)
            return;
        doc!.CreateMask(member.GuidValue);
    }

    [Command.Basic("PixiEditor.Layer.DeleteMask", "Delete mask", "Delete mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask")]
    public void DeleteMask(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        doc!.DeleteMask(member.GuidValue);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanEnableClipToMemberBelow")]
    public bool CanClipToMemberBelow() => !Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.ClipToMemberBelowEnabledBindable ?? false;
    [Evaluator.CanExecute("PixiEditor.Layer.CanDisableClipToMemberBelow")]
    public bool CanDisableClipToMemberBelow() => Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.ClipToMemberBelowEnabledBindable ?? false;

    [Command.Basic("PixiEditor.Layer.EnableClipToMemberBelow", "Clip to layer below", "Clip to layer below", Parameter = true, CanExecute = "PixiEditor.Layer.CanEnableClipToMemberBelow")]
    [Command.Basic("PixiEditor.Layer.DisableClipToMemberBelow", "Disable clip to layer below", "Disable clip to layer below", Parameter = false, CanExecute = "PixiEditor.Layer.CanDisableClipToMemberBelow")]
    public void ToogleClipToMemberBelow(bool newValue)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null || member.ClipToMemberBelowEnabledBindable == newValue)
            return;
        member.ClipToMemberBelowEnabledBindable = newValue;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberAbove")]
    public bool HasMemberAbove(object property) => HasSelectedMember(true);
    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberBelow")]
    public bool HasMemberBelow(object property) => HasSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberUpwards", "Move selected layer upwards", "Move selected layer or folder upwards", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MoveSelectedMemberUpwards(object parameter) => MoveSelectedMember(true);
    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberDownwards", "Move selected layer downwards", "Move selected layer or folder downwards", CanExecute = "PixiEditor.Layer.HasMemberBelow")]
    public void MoveSelectedMemberDownwards(object parameter) => MoveSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MergeSelected", "Merge all selected layers", "Merge all selected layers", CanExecute = "PixiEditor.Layer.HasMultipleSelectedMembers")]
    public void MergeSelected(object parameter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = GetSelected();
        if (selected.Count == 0)
            return;
        doc.MergeStructureMembers(selected);
    }

    public void MergeSelectedWith(bool above)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (doc is null || member is null)
            return;
        var (child, parent) = doc.StructureViewModel.FindChildAndParent(member.GuidValue);
        if (child is null || parent is null)
            return;
        int index = parent.Children.IndexOf(child);
        if (!above && index == 0)
            return;
        if (above && index == parent.Children.Count - 1)
            return;
        doc.MergeStructureMembers(new List<Guid> { member.GuidValue, above ? parent.Children[index + 1].GuidValue : parent.Children[index - 1].GuidValue });
    }

    [Command.Basic("PixiEditor.Layer.MergeWithAbove", "Merge selected layer with the one above it", "Merge selected layer with the one above it", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MergeWithAbove(object parameter) => MergeSelectedWith(true);

    [Command.Basic("PixiEditor.Layer.MergeWithBelow", "Merge selected layer with the one below it", "Merge selected layer with the one below it", CanExecute = "PixiEditor.Layer.HasMemberBelow")]
    public void MergeWithBelow(object parameter) => MergeSelectedWith(false);
}
