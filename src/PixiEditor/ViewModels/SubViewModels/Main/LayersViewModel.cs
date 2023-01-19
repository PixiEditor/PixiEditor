using System.Collections.Immutable;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Layer", "Image")]
internal class LayersViewModel : SubViewModel<ViewModelMain>
{
    public LayersViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    public void CreateFolderFromActiveLayers()
    {

    }

    public bool CanCreateFolderFromSelected()
    {
        return false;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDeleteSelected")]
    public bool CanDeleteSelected()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return false;
        return true;
    }

    [Command.Basic("PixiEditor.Layer.DeleteSelected", "Delete active layer/folder", "Delete active layer or folder", CanExecute = "PixiEditor.Layer.CanDeleteSelected", IconPath = "Trash.png")]
    public void DeleteSelected()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return;
        member.Document.Operations.DeleteStructureMember(member.GuidValue);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasSelectedMembers")]
    public bool HasSelectedMembers()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.SelectedStructureMember is not null || doc.SoftSelectedStructureMembers.Count > 0;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasMultipleSelectedMembers")]
    public bool HasMultipleSelectedMembers()
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
        List<Guid> members = new();
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return members;
        if (doc.SelectedStructureMember is not null)
            members.Add(doc.SelectedStructureMember.GuidValue);
        members.AddRange(doc.SoftSelectedStructureMembers.Select(static member => member.GuidValue));
        return members;
    }

    [Command.Basic("PixiEditor.Layer.DeleteAllSelected", "Delete all selected layers/folders", "Delete all selected layers and/or folders", CanExecute = "PixiEditor.Layer.HasSelectedMembers", IconPath = "Trash.png")]
    public void DeleteAllSelected()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = GetSelected();
        if (selected.Count > 0)
            doc.Operations.DeleteStructureMembers(selected);
    }

    [Command.Basic("PixiEditor.Layer.NewFolder", "New Folder", "Create new folder", CanExecute = "PixiEditor.Layer.CanCreateNewMember", IconPath = "Folder-add.png")]
    public void NewFolder()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.Operations.CreateStructureMember(StructureMemberType.Folder);
    }

    [Command.Basic("PixiEditor.Layer.NewLayer", "New Layer", "Create new layer", CanExecute = "PixiEditor.Layer.CanCreateNewMember", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
    public void NewLayer()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.Operations.CreateStructureMember(StructureMemberType.Layer);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanCreateNewMember")]
    public bool CanCreateNewMember()
    {
        return Owner.DocumentManagerSubViewModel.ActiveDocument is { } doc && !doc.UpdateableChangeActive;
    }

    [Command.Internal("PixiEditor.Layer.ToggleLockTransparency", CanExecute = "PixiEditor.Layer.SelectedMemberIsLayer")]
    public void ToggleLockTransparency()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is not LayerViewModel layerVm)
            return;
        layerVm.LockTransparencyBindable = !layerVm.LockTransparencyBindable;
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragStarted")]
    public void OpacitySliderDragStarted()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseOpacitySlider();
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnOpacitySliderDragStarted();
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragged")]
    public void OpacitySliderDragged(double value)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnOpacitySliderDragged((float)value);
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragEnded")]
    public void OpacitySliderDragEnded()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnOpacitySliderDragEnded();
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderSet")]
    public void OpacitySliderSet(double value)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (document.SelectedStructureMember != null)
        {
            document?.Operations.SetMemberOpacity(document.SelectedStructureMember.GuidValue, (float)value);
        }
    }

    [Command.Basic("PixiEditor.Layer.DuplicateSelectedLayer", "Duplicate selected layer", "Duplicate selected layer", CanExecute = "PixiEditor.Layer.SelectedMemberIsLayer")]
    public void DuplicateLayer()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is not LayerViewModel layerVM)
            return;
        member.Document.Operations.DuplicateLayer(member.GuidValue);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsLayer")]
    public bool SelectedMemberIsLayer(object property)
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
        var path = doc!.StructureHelper.FindPath(member.GuidValue);
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
        var path = doc!.StructureHelper.FindPath(member.GuidValue);
        if (path.Count < 2)
            return;
        var parent = (FolderViewModel)path[1];
        int curIndex = parent.Children.IndexOf(path[0]);
        if (upwards)
        {
            if (curIndex == parent.Children.Count - 1)
                return;
            doc.Operations.MoveStructureMember(member.GuidValue, parent.Children[curIndex + 1].GuidValue, StructureMemberPlacement.Above);
        }
        else
        {
            if (curIndex == 0)
                return;
            doc.Operations.MoveStructureMember(member.GuidValue, parent.Children[curIndex - 1].GuidValue, StructureMemberPlacement.Below);
        }
    }

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasMask")]
    public bool ActiveMemberHasMask() => Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false;

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasNoMask")]
    public bool ActiveLayerHasNoMask() => !Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false;

    [Command.Basic("PixiEditor.Layer.CreateMask", "Create mask", "Create mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasNoMask", IconPath = "Create-mask.png")]
    public void CreateMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || member.HasMaskBindable)
            return;
        doc!.Operations.CreateMask(member);
    }

    [Command.Basic("PixiEditor.Layer.DeleteMask", "Delete mask", "Delete mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask", IconPath = "Trash.png")]
    public void DeleteMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        doc!.Operations.DeleteMask(member);
    }

    [Command.Basic("PixiEditor.Layer.ToggleMask", "Toggle mask", "Toggle mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask")]
    public void ToggleMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        
        member.MaskIsVisibleBindable = !member.MaskIsVisibleBindable;
    }
    
    [Command.Basic("PixiEditor.Layer.ApplyMask", "Apply mask", "Apply mask", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask")]
    public void ApplyMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        
        doc!.Operations.ApplyMask(member);
    }

    [Command.Basic("PixiEditor.Layer.ToggleVisible", "Toggle visibility", "Toggle visibility", CanExecute = "PixiEditor.HasDocument")]
    public void ToggleVisible()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return;
        
        member.IsVisibleBindable = !member.IsVisibleBindable;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberAbove")]
    public bool HasMemberAbove(object property) => HasSelectedMember(true);
    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberBelow")]
    public bool HasMemberBelow(object property) => HasSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberUpwards", "Move selected layer upwards", "Move selected layer or folder upwards", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MoveSelectedMemberUpwards() => MoveSelectedMember(true);
    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberDownwards", "Move selected layer downwards", "Move selected layer or folder downwards", CanExecute = "PixiEditor.Layer.HasMemberBelow")]
    public void MoveSelectedMemberDownwards() => MoveSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MergeSelected", "Merge all selected layers", "Merge all selected layers", CanExecute = "PixiEditor.Layer.HasMultipleSelectedMembers")]
    public void MergeSelected()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = GetSelected();
        if (selected.Count == 0)
            return;
        doc.Operations.MergeStructureMembers(selected);
    }

    public void MergeSelectedWith(bool above)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (doc is null || member is null)
            return;
        var (child, parent) = doc.StructureHelper.FindChildAndParent(member.GuidValue);
        if (child is null || parent is null)
            return;
        int index = parent.Children.IndexOf(child);
        if (!above && index == 0)
            return;
        if (above && index == parent.Children.Count - 1)
            return;
        doc.Operations.MergeStructureMembers(new List<Guid> { member.GuidValue, above ? parent.Children[index + 1].GuidValue : parent.Children[index - 1].GuidValue });
    }

    [Command.Basic("PixiEditor.Layer.MergeWithAbove", "Merge selected layer with the one above it", "Merge selected layer with the one above it", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MergeWithAbove() => MergeSelectedWith(true);

    [Command.Basic("PixiEditor.Layer.MergeWithBelow", "Merge selected layer with the one below it", "Merge selected layer with the one below it", CanExecute = "PixiEditor.Layer.HasMemberBelow", IconPath = "Merge-downwards.png")]
    public void MergeWithBelow() => MergeSelectedWith(false);

    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerExists")]
    public bool ReferenceLayerExists() => Owner.DocumentManagerSubViewModel.ActiveDocument?.ReferenceLayerViewModel.ReferenceBitmap is not null;
    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerDoesntExist")]
    public bool ReferenceLayerDoesntExist() => 
        Owner.DocumentManagerSubViewModel.ActiveDocument is null ? false : Owner.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel.ReferenceBitmap is null;

    [Command.Basic("PixiEditor.Layer.ImportReferenceLayer", "Add reference layer", "Add reference layer", CanExecute = "PixiEditor.Layer.ReferenceLayerDoesntExist")]
    public void ImportReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        string path = OpenReferenceLayerFilePicker();
        if (path is null)
            return;

        WriteableBitmap bitmap;
        try
        {
            bitmap = Importer.ImportWriteableBitmap(path);
        }
        catch (Exception e)
        {
            NoticeDialog.Show("Error while importing the image", "Error");
            return;
        }

        byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
        bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);

        VecI size = new VecI(bitmap.PixelWidth, bitmap.PixelHeight);

        doc.Operations.ImportReferenceLayer(
            pixels.ToImmutableArray(), 
            size);
    }
    private string OpenReferenceLayerFilePicker()
    {
        var imagesFilter = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Images).GetFormattedTypes();
        OpenFileDialog dialog = new OpenFileDialog
        {
            Title = "Reference layer path",
            CheckPathExists = true,
            Filter = imagesFilter
        };

        return (bool)dialog.ShowDialog() ? dialog.FileName : null;
    }

    [Command.Basic("PixiEditor.Layer.DeleteReferenceLayer", "Delete reference layer", "Delete reference layer", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "Trash.png")]
    public void DeleteReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.DeleteReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.TransformReferenceLayer", "Transform reference layer", "Transform reference layer", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "Tools/MoveImage.png")]
    public void TransformReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.TransformReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.ResetReferenceLayerPosition", "Reset reference layer position", "Reset reference layer position", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "Layout.png")]
    public void ResetReferenceLayerPosition()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.ResetReferenceLayerPosition();
    }

}
