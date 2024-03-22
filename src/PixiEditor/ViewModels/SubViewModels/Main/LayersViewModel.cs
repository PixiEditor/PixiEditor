using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Exceptions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls.Layers;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
[Command.Group("PixiEditor.Layer", "LAYER")]
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

    [Command.Basic("PixiEditor.Layer.DeleteSelected", "LAYER_DELETE_SELECTED", "LAYER_DELETE_SELECTED_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.CanDeleteSelected", IconPath = "Trash.png")]
    public void DeleteSelected()
    {
        var doc = Owner.DocumentManagerSubViewModel?.ActiveDocument;

        var member = doc.SelectedStructureMember;
        if (member is null)
            return;

        member.Document.Operations.DeleteStructureMember(member.GuidValue);

        var allLayers = doc.StructureHelper.GetAllLayers().ToArray();

        if (allLayers.Length > 1)
        {
            //Declare the layer that will be selected
             var baseLayer = allLayers[allLayers.ToArray().Length - 1];
        
             for (int i = 0; i <allLayers.Length; i++)
            {
                //Checking that the selected layer and the deleted layer are not the same
                if (allLayers[i] == member)
                {
                    //Selecting the new layer
                    if (i > 0)
                    {
                        baseLayer = allLayers[i - 1];
                    }
                    else if (i == allLayers.Length - 1) 
                    {
                        baseLayer = allLayers[i];
                    }
                    else if (i == 0)
                    {
                        baseLayer = allLayers[i + 1];
                    }

                        break;
                }
             }
            doc.Operations.AddSoftSelectedMember(baseLayer.GuidValue);
            doc.InternalSetSelectedMember(baseLayer);
        }
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

    [Command.Basic("PixiEditor.Layer.DeleteAllSelected", "LAYER_DELETE_ALL_SELECTED", "LAYER_DELETE_ALL_SELECTED_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasSelectedMembers", IconPath = "Trash.png")]
    public void DeleteAllSelected()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = GetSelected();
        if (selected.Count > 0)
            doc.Operations.DeleteStructureMembers(selected);
    }

    [Command.Basic("PixiEditor.Layer.NewFolder", "NEW_FOLDER", "CREATE_NEW_FOLDER", CanExecute = "PixiEditor.Layer.CanCreateNewMember", IconPath = "Folder-add.png")]
    public void NewFolder()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;
        doc.Operations.CreateStructureMember(StructureMemberType.Folder);
    }

    [Command.Basic("PixiEditor.Layer.NewLayer", "NEW_LAYER", "CREATE_NEW_LAYER", CanExecute = "PixiEditor.Layer.CanCreateNewMember", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
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

        if (document?.SelectedStructureMember != null)
        {
            document.Operations.SetMemberOpacity(document.SelectedStructureMember.GuidValue, (float)value);
        }
    }

    [Command.Basic("PixiEditor.Layer.DuplicateSelectedLayer", "DUPLICATE_SELECTED_LAYER", "DUPLICATE_SELECTED_LAYER", CanExecute = "PixiEditor.Layer.SelectedMemberIsLayer")]
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

    [Command.Basic("PixiEditor.Layer.CreateMask", "CREATE_MASK", "CREATE_MASK", CanExecute = "PixiEditor.Layer.ActiveLayerHasNoMask", IconPath = "Create-mask.png")]
    public void CreateMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || member.HasMaskBindable)
            return;
        doc!.Operations.CreateMask(member);
    }

    [Command.Basic("PixiEditor.Layer.DeleteMask", "DELETE_MASK", "DELETE_MASK", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask", IconPath = "Trash.png")]
    public void DeleteMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        doc!.Operations.DeleteMask(member);
    }

    [Command.Basic("PixiEditor.Layer.ToggleMask", "TOGGLE_MASK", "TOGGLE_MASK", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask")]
    public void ToggleMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        
        member.MaskIsVisibleBindable = !member.MaskIsVisibleBindable;
    }
    
    [Command.Basic("PixiEditor.Layer.ApplyMask", "APPLY_MASK", "APPLY_MASK", CanExecute = "PixiEditor.Layer.ActiveLayerHasMask")]
    public void ApplyMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        
        doc!.Operations.ApplyMask(member);
    }

    [Command.Basic("PixiEditor.Layer.ToggleVisible", "TOGGLE_VISIBILITY", "TOGGLE_VISIBILITY", CanExecute = "PixiEditor.HasDocument")]
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

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberUpwards", "MOVE_MEMBER_UP", "MOVE_MEMBER_UP_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MoveSelectedMemberUpwards() => MoveSelectedMember(true);
    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberDownwards", "MOVE_MEMBER_DOWN", "MOVE_MEMBER_DOWN_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasMemberBelow")]
    public void MoveSelectedMemberDownwards() => MoveSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MergeSelected", "MERGE_ALL_SELECTED_LAYERS", "MERGE_ALL_SELECTED_LAYERS", CanExecute = "PixiEditor.Layer.HasMultipleSelectedMembers")]
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

    [Command.Basic("PixiEditor.Layer.MergeWithAbove", "MERGE_WITH_ABOVE", "MERGE_WITH_ABOVE_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasMemberAbove")]
    public void MergeWithAbove() => MergeSelectedWith(true);

    [Command.Basic("PixiEditor.Layer.MergeWithBelow", "MERGE_WITH_BELOW", "MERGE_WITH_BELOW_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasMemberBelow", IconPath = "Merge-downwards.png")]
    public void MergeWithBelow() => MergeSelectedWith(false);

    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerExists")]
    public bool ReferenceLayerExists() => Owner.DocumentManagerSubViewModel.ActiveDocument?.ReferenceLayerViewModel.ReferenceBitmap is not null;
    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerDoesntExist")]
    public bool ReferenceLayerDoesntExist() => 
        Owner.DocumentManagerSubViewModel.ActiveDocument is not null && Owner.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel.ReferenceBitmap is null;

    [Command.Basic("PixiEditor.Layer.ImportReferenceLayer", "ADD_REFERENCE_LAYER", "ADD_REFERENCE_LAYER", CanExecute = "PixiEditor.Layer.ReferenceLayerDoesntExist", IconPath = "Add-reference.png")]
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
        catch (RecoverableException e)
        {
            NoticeDialog.Show(title: "ERROR", message: e.DisplayMessage);
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
            Title = new LocalizedString("REFERENCE_LAYER_PATH"),
            CheckPathExists = true,
            Filter = imagesFilter
        };

        return (bool)dialog.ShowDialog() ? dialog.FileName : null;
    }

    [Command.Basic("PixiEditor.Layer.DeleteReferenceLayer", "DELETE_REFERENCE_LAYER", "DELETE_REFERENCE_LAYER", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "Trash.png")]
    public void DeleteReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.DeleteReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.TransformReferenceLayer", "TRANSFORM_REFERENCE_LAYER", "TRANSFORM_REFERENCE_LAYER", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "crop.png")]
    public void TransformReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.TransformReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.ToggleReferenceLayerTopMost", "TOGGLE_REFERENCE_LAYER_POS", "TOGGLE_REFERENCE_LAYER_POS_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconEvaluator = "PixiEditor.Layer.ToggleReferenceLayerTopMostIcon")]
    public void ToggleReferenceLayerTopMost()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.ReferenceLayerViewModel.IsTopMost = !doc.ReferenceLayerViewModel.IsTopMost;
    }

    [Command.Basic("PixiEditor.Layer.ResetReferenceLayerPosition", "RESET_REFERENCE_LAYER_POS", "RESET_REFERENCE_LAYER_POS", CanExecute = "PixiEditor.Layer.ReferenceLayerExists", IconPath = "Layout.png")]
    public void ResetReferenceLayerPosition()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.ResetReferenceLayerPosition();
    }

    [Evaluator.Icon("PixiEditor.Layer.ToggleReferenceLayerTopMostIcon")]
    public ImageSource GetAboveEverythingReferenceLayerIcon()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || doc.ReferenceLayerViewModel.IsTopMost)
            return new BitmapImage(new Uri("pack://application:,,,/Images/ReferenceLayerBelow.png"));

        return new BitmapImage(new Uri("pack://application:,,,/Images/ReferenceLayerAbove.png"));
    }
}
