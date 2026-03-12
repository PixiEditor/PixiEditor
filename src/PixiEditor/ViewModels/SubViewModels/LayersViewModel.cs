using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument;
using PixiEditor.Helpers.Converters;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Helpers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.Views.Overlays.TextOverlay;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
[Command.Group("PixiEditor.Layer", "LAYER")]
internal class LayersViewModel : SubViewModel<ViewModelMain>
{
    public LayersViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanDeleteSelected",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember))]
    public bool CanDeleteSelected()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is null)
            return false;
        return true;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasSelectedMembers",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember),
        nameof(DocumentManagerViewModel.ActiveDocument.SoftSelectedStructureMembers))]
    public bool HasSelectedMembers()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return doc.SelectedStructureMember is not null || doc.SoftSelectedStructureMembers.Count > 0;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasTransformableMembers",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember),
        nameof(DocumentManagerViewModel.ActiveDocument.SoftSelectedStructureMembers))]
    public bool HasTransformableMembers()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        bool hasMembers = doc.SelectedStructureMember is not null || doc.SoftSelectedStructureMembers.Count > 0;
        if (!hasMembers)
            return false;

        var selectedMembers = doc.ExtractSelectedLayers();
        foreach (var member in selectedMembers)
        {
            var handler = doc.StructureHelper.Find(member);
            if (handler is ITransformableMemberHandler)
            {
                return true;
            }
        }

        return false;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.IsMemberTransformable",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember),
        nameof(DocumentManagerViewModel.ActiveDocument.SoftSelectedStructureMembers))]
    public bool IsMemberTransformable(object property)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;

        if(property is Guid memberGuid)
        {
            var handler = doc.StructureHelper.Find(memberGuid);
            return handler is ITransformableMemberHandler;
        }

        return false;
    }


    [Evaluator.CanExecute("PixiEditor.Layer.HasMultipleSelectedMembers",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentManagerViewModel.ActiveDocument.SelectedStructureMember),
        nameof(DocumentManagerViewModel.ActiveDocument.SoftSelectedStructureMembers))]
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
            members.Add(doc.SelectedStructureMember.Id);
        members.AddRange(doc.SoftSelectedStructureMembers.Select(static member => member.Id));
        return members;
    }

    [Command.Basic("PixiEditor.Layer.DeleteAllSelected", "LAYER_DELETE_ALL_SELECTED",
        "LAYER_DELETE_ALL_SELECTED_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.HasSelectedMembers",
        Icon = PixiPerfectIcons.Trash, AnalyticsTrack = true, Key = Key.Delete,
        ShortcutContexts = [typeof(LayersDockViewModel)])]
    public void DeleteAllSelected()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        var selected = doc.ExtractSelectedLayers(true).Concat(doc.SelectedMembers).Distinct().ToList();
        if (selected.Count > 0)
        {
            doc.Operations.DeleteStructureMembers(selected);
        }
    }

    [Command.Basic("PixiEditor.Layer.NewFolder", "NEW_FOLDER", "CREATE_NEW_FOLDER",
        CanExecute = "PixiEditor.Layer.CanCreateNewMember",
        Icon = PixiPerfectIcons.FolderPlus, AnalyticsTrack = true)]
    public void NewFolder()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;

        using var block = doc.Operations.StartChangeBlock();
        Guid? guid = doc.Operations.CreateStructureMember(StructureMemberType.Folder);
        if (doc.SoftSelectedStructureMembers.Count == 0)
            return;
        var selectedInOrder = doc.GetSelectedMembersInOrder();
        selectedInOrder.Reverse();
        block.ExecuteQueuedActions();

        if (guid is null)
            return;

        if (selectedInOrder.Count > 0)
        {
            Guid lastMovedMember = guid.Value;
            StructureMemberPlacement placement = StructureMemberPlacement.Inside;

            foreach (Guid memberGuid in selectedInOrder)
            {
                doc.Operations.MoveStructureMember(memberGuid, lastMovedMember, placement);
                lastMovedMember = memberGuid;
                if (placement == StructureMemberPlacement.Inside)
                {
                    placement = StructureMemberPlacement.Below;
                }

                block.ExecuteQueuedActions();
            }

            doc.Operations.ClearSoftSelectedMembers();
        }

        doc.Operations.SetSelectedMember(guid.Value);
    }

    [Command.Basic("PixiEditor.Layer.NewLayer", "NEW_LAYER", "CREATE_NEW_LAYER",
        CanExecute = "PixiEditor.Layer.CanCreateNewMember", Key = Key.N,
        Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Icon = PixiPerfectIcons.FilePlus, AnalyticsTrack = true)]
    public void NewLayer()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return;

        doc.Operations.CreateStructureMember(StructureMemberType.ImageLayer);
    }

    public Guid? NewLayer(Type layerType, ActionSource source, string? name = null)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return null;

        return doc.Operations.CreateStructureMember(layerType, source, name);
    }

    public Guid? ForceNewLayer(Type layerType, ActionSource source, string? name = null)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not { } doc)
            return null;

        return doc.Operations.ForceCreateStructureMember(layerType, source, name);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.CanCreateNewMember")]
    public bool CanCreateNewMember()
    {
        return Owner.DocumentManagerSubViewModel.ActiveDocument is { BlockingUpdateableChangeActive: false };
    }

    [Command.Internal("PixiEditor.Layer.ToggleLockTransparency",
        CanExecute = "PixiEditor.Layer.SelectedMemberIsTransparencyLockable",
        AnalyticsTrack = true)]
    public void ToggleLockTransparency()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is not ImageLayerNodeViewModel layerVm)
            return;
        layerVm.LockTransparencyBindable = !layerVm.LockTransparencyBindable;
    }

    [Command.Internal("PixiEditor.Layer.SelectActiveMember", AnalyticsTrack = true)]
    public void SelectActiveMember(Guid memberGuid)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        var member = doc.StructureHelper.Find(memberGuid);
        if (member is null)
            return;

        doc.Operations.SetSelectedMember(member.Id);
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

    [Command.Internal("PixiEditor.Layer.OpacitySliderDragEnded", AnalyticsTrack = true)]
    public void OpacitySliderDragEnded()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnOpacitySliderDragEnded();
    }

    [Command.Internal("PixiEditor.Layer.OpacitySliderSet", AnalyticsTrack = true)]
    public void OpacitySliderSet(double value)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;

        if (document?.SelectedStructureMember != null)
        {
            document.Operations.SetMemberOpacity(document.SelectedStructureMember.Id, (float)value);
        }
    }

    [Command.Basic("PixiEditor.Layer.DuplicateSelectedMember", "DUPLICATE_SELECTED_LAYER", "DUPLICATE_SELECTED_LAYER",
        Icon = PixiPerfectIcons.DuplicateFile, MenuItemPath = "EDIT/DUPLICATE", MenuItemOrder = 5,
        AnalyticsTrack = true)]
    public void DuplicateMember()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember == null)
            return;

        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;

        member.Document.Operations.DuplicateMember(member.Id);
    }

    [Command.Basic("PixiEditor.Layer.UnlinkNestedDocument", "UNLINK", "UNLINK_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.SelectedMemberIsNestedDocument",
        Icon = PixiPerfectIcons.ChainBreak, AnalyticsTrack = true)]
    public void UnlinkNestedDocument()
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        if (member is not NestedDocumentNodeViewModel nestedDocVm)
            return;

        nestedDocVm.Document.Operations.UnlinkNestedDocument(nestedDocVm.Id);
    }

    [Command.Internal("PixiEditor.Layer.CreateNestedFromLayer")]
    public void CreateNestedFromLayer(Guid layerGuid)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.CreateNestedDocumentFromMember(layerGuid);
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsLayer",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedMemberIsLayer(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is ILayerHandler;
    }


    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsTransparencyLockable",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedMemberIsTransparencyLockable(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is ITransparencyLockableMember;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedLayerIsRasterizable",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedLayerIsRasterizable(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is ILayerHandler && member is not IRasterLayerHandler;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsVectorLayer",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedMemberIsVectorLayer(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is IVectorLayerHandler;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.AnySelectedMemberIsVectorLayer",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool AnySelectedMemberIsVectorLayer(object property)
    {
        var members = Owner.DocumentManagerSubViewModel.ActiveDocument?.ExtractSelectedLayers();

        if (members is null || members.Count == 0)
            return false;

        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;

        foreach (var member in members)
        {
            var handler = doc.StructureHelper.Find(member);
            if (handler is IVectorLayerHandler)
            {
                return true;
            }
        }

        return false;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.AnySelectedLayerIsRasterizable",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool AnySelectedMemberIsRasterizable(object property)
    {
        var members = Owner.DocumentManagerSubViewModel.ActiveDocument?.ExtractSelectedLayers();

        if (members is null || members.Count == 0)
            return false;

        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;

        foreach (var member in members)
        {
            var handler = doc.StructureHelper.Find(member);
            if (handler is ILayerHandler && handler is not IRasterLayerHandler)
            {
                return true;
            }
        }

        return false;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsNestedDocument",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedMemberIsNestedDocument(object property)
    {
        var member = Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember;
        return member is NestedDocumentNodeViewModel;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.SelectedMemberIsSelectedText",
        nameof(DocumentManagerViewModel.ActiveDocument), nameof(DocumentViewModel.SelectedStructureMember))]
    public bool SelectedMemberIsSelectedText(object property)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;

        var member = doc?.SelectedStructureMember;
        return member is IVectorLayerHandler && doc.TextOverlayViewModel.IsActive &&
               doc.TextOverlayViewModel.CursorPosition != doc.TextOverlayViewModel.SelectionEnd;
    }

    private bool HasSelectedMember(bool above)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return false;
        if (above)
        {
            return doc.StructureHelper.GetAboveMember(member.Id, false) is not null;
        }

        return doc.StructureHelper.GetBelowMember(member.Id, false) is not null;
    }

    private void MoveSelectedMember(bool upwards)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return;
        var path = doc!.StructureHelper.FindPath(member.Id);
        if (path.Count < 2 || path[1] is not FolderNodeViewModel folderVm)
            return;
        var parent = folderVm;
        if (parent.Children.Count == 0)
            return;
        int curIndex = parent.Children.IndexOf(path[0]);
        if (upwards)
        {
            if (curIndex == parent.Children.Count - 1)
                return;
            doc.Operations.MoveStructureMember(member.Id, parent.Children[curIndex + 1].Id,
                StructureMemberPlacement.Above);
        }
        else
        {
            if (curIndex == 0)
                return;
            doc.Operations.MoveStructureMember(member.Id, parent.Children[curIndex - 1].Id,
                StructureMemberPlacement.Below);
        }
    }

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasMask",
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember.HasMaskBindable))]
    public bool ActiveMemberHasMask() =>
        Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false;

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasApplyableMask",
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember.HasMaskBindable))]
    public bool ActiveMemberHasApplyableMask() =>
        (Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false)
        && Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember is IRasterLayerHandler;

    [Evaluator.CanExecute("PixiEditor.Layer.ActiveLayerHasNoMask",
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.SelectedStructureMember.HasMaskBindable))]
    public bool ActiveLayerHasNoMask() =>
        !Owner.DocumentManagerSubViewModel.ActiveDocument?.SelectedStructureMember?.HasMaskBindable ?? false;

    [Command.Basic("PixiEditor.Layer.CreateMask", "CREATE_MASK", "CREATE_MASK",
        CanExecute = "PixiEditor.Layer.ActiveLayerHasNoMask",
        Icon = PixiPerfectIcons.CreateMask, AnalyticsTrack = true)]
    public void CreateMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || member.HasMaskBindable)
            return;
        doc!.Operations.CreateMask(member);
    }

    [Command.Basic("PixiEditor.Layer.DeleteMask", "DELETE_MASK", "DELETE_MASK",
        CanExecute = "PixiEditor.Layer.ActiveLayerHasMask", Icon = PixiPerfectIcons.Trash, AnalyticsTrack = true)]
    public void DeleteMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;
        doc!.Operations.DeleteMask(member);
    }

    [Command.Basic("PixiEditor.Layer.ToggleMask", "TOGGLE_MASK", "TOGGLE_MASK",
        CanExecute = "PixiEditor.Layer.ActiveLayerHasMask",
        Icon = PixiPerfectIcons.MaskGhost, AnalyticsTrack = true)]
    public void ToggleMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;

        member.MaskIsVisibleBindable = !member.MaskIsVisibleBindable;
    }

    [Command.Basic("PixiEditor.Layer.ApplyMask", "APPLY_MASK", "APPLY_MASK",
        CanExecute = "PixiEditor.Layer.ActiveLayerHasApplyableMask", AnalyticsTrack = true)]
    public void ApplyMask()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null || !member.HasMaskBindable)
            return;

        doc!.Operations.ApplyMask(member, doc.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Layer.ToggleVisible", "TOGGLE_VISIBILITY", "TOGGLE_VISIBILITY",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.FileGhost, AnalyticsTrack = true)]
    public void ToggleVisible()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return;

        member.IsVisibleBindable = !member.IsVisibleBindable;
    }

    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberAbove",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentViewModel.SelectedStructureMember), nameof(DocumentViewModel.AllChangesSaved))]
    public bool HasMemberAbove(object property) => HasSelectedMember(true);

    [Evaluator.CanExecute("PixiEditor.Layer.HasMemberBelow",
        nameof(DocumentManagerViewModel.ActiveDocument),
        nameof(DocumentViewModel.SelectedStructureMember), nameof(DocumentViewModel.AllChangesSaved))]
    public bool HasMemberBelow(object property) => HasSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberUpwards", "MOVE_MEMBER_UP", "MOVE_MEMBER_UP_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.HasMemberAbove", AnalyticsTrack = true)]
    public void MoveSelectedMemberUpwards() => MoveSelectedMember(true);

    [Command.Basic("PixiEditor.Layer.MoveSelectedMemberDownwards", "MOVE_MEMBER_DOWN", "MOVE_MEMBER_DOWN_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.HasMemberBelow", AnalyticsTrack = true)]
    public void MoveSelectedMemberDownwards() => MoveSelectedMember(false);

    [Command.Basic("PixiEditor.Layer.MergeSelected", "MERGE_ALL_SELECTED_LAYERS", "MERGE_ALL_SELECTED_LAYERS",
        CanExecute = "PixiEditor.Layer.HasMultipleSelectedMembers", AnalyticsTrack = true)]
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

        IStructureMemberHandler? nextMergeableMember = doc.StructureHelper.GetAboveMember(member.Id, true);
        IStructureMemberHandler? previousMergeableMember = doc.StructureHelper.GetBelowMember(member.Id, true);

        if (!above && previousMergeableMember is null)
            return;
        if (above && nextMergeableMember is null)
            return;

        doc.Operations.MergeStructureMembers(new List<Guid>
        {
            member.Id, above ? nextMergeableMember.Id : previousMergeableMember.Id
        });
    }

    [Command.Basic("PixiEditor.Layer.MergeWithAbove", "MERGE_WITH_ABOVE", "MERGE_WITH_ABOVE_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.HasMemberAbove", AnalyticsTrack = true)]
    public void MergeWithAbove() => MergeSelectedWith(true);

    [Command.Basic("PixiEditor.Layer.MergeWithBelow", "MERGE_WITH_BELOW", "MERGE_WITH_BELOW_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.HasMemberBelow",
        Icon = PixiPerfectIcons.Merge, AnalyticsTrack = true)]
    public void MergeWithBelow() => MergeSelectedWith(false);

    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerExists",
        nameof(ViewModelMain.DocumentManagerSubViewModel),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel.ReferenceTexture))]
    public bool ReferenceLayerExists() =>
        Owner.DocumentManagerSubViewModel.ActiveDocument?.ReferenceLayerViewModel.ReferenceTexture is not null;

    [Evaluator.CanExecute("PixiEditor.Layer.ReferenceLayerDoesntExist",
        nameof(ViewModelMain.DocumentManagerSubViewModel),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument),
        nameof(ViewModelMain.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel.ReferenceTexture))]
    public bool ReferenceLayerDoesntExist() =>
        Owner.DocumentManagerSubViewModel.ActiveDocument is not null &&
        Owner.DocumentManagerSubViewModel.ActiveDocument.ReferenceLayerViewModel.ReferenceTexture is null;

    [Command.Basic("PixiEditor.Layer.ImportReferenceLayer", "ADD_REFERENCE_LAYER", "ADD_REFERENCE_LAYER",
        CanExecute = "PixiEditor.Layer.ReferenceLayerDoesntExist",
        Icon = PixiPerfectIcons.AddReference, AnalyticsTrack = true)]
    public async Task ImportReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        string path = await OpenReferenceLayerFilePicker();
        if (path is null)
            return;

        Surface bitmap;
        try
        {
            bitmap = Surface.Load(path);
        }
        catch (RecoverableException e)
        {
            NoticeDialog.Show(title: "ERROR", message: e.DisplayMessage);
            return;
        }
        catch (ArgumentException e)
        {
            NoticeDialog.Show(title: "ERROR", message: e.Message);
            return;
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
            NoticeDialog.Show(title: "ERROR", message: e.Message);
            return;
        }

        byte[] bytes = bitmap.ToByteArray();

        bitmap.Dispose();

        VecI size = new VecI(bitmap.Size.X, bitmap.Size.Y);

        doc.Operations.ImportReferenceLayer(
            [..bytes],
            size);
    }

    private async Task<string> OpenReferenceLayerFilePicker()
    {
        var imagesFilter = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Image).GetFormattedTypes(true);
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var filePicker = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = new LocalizedString("REFERENCE_LAYER_PATH"), FileTypeFilter = imagesFilter,
            });

            if (filePicker is null || filePicker.Count == 0)
                return null;

            return filePicker[0].Path.LocalPath;
        }

        return null;
    }

    [Command.Basic("PixiEditor.Layer.ResetTransformForSelected", "RESET_TRANSFORM", "RESET_TRANSFORM_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.HasTransformableMembers", Icon = PixiPerfectIcons.Reset, AnalyticsTrack = true)]
    public void ResetTransform()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        var selectedMembers = doc.ExtractSelectedLayers();
        if (selectedMembers.Count == 0)
            return;

        var block = doc.Operations.StartChangeBlock();

        foreach (var member in selectedMembers)
        {
            doc.Operations.ResetTransform(member);
        }

        block.Dispose();
    }

    [Command.Internal("PixiEditor.Layer.ResetTransformForMember",
        CanExecute = "PixiEditor.Layer.IsMemberTransformable", AnalyticsTrack = true)]
    public void ResetTransform(Guid memberGuid)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.ResetTransform(memberGuid);
    }

    [Command.Basic("PixiEditor.Layer.DeleteReferenceLayer", "DELETE_REFERENCE_LAYER", "DELETE_REFERENCE_LAYER",
        CanExecute = "PixiEditor.Layer.ReferenceLayerExists", Icon = PixiPerfectIcons.Trash, AnalyticsTrack = true)]
    public void DeleteReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.DeleteReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.TransformReferenceLayer", "TRANSFORM_REFERENCE_LAYER", "TRANSFORM_REFERENCE_LAYER",
        CanExecute = "PixiEditor.Layer.ReferenceLayerExists",
        Icon = PixiPerfectIcons.Crop, AnalyticsTrack = true)]
    public void TransformReferenceLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.TransformReferenceLayer();
    }

    [Command.Basic("PixiEditor.Layer.ToggleReferenceLayerTopMost", "TOGGLE_REFERENCE_LAYER_POS",
        "TOGGLE_REFERENCE_LAYER_POS_DESCRIPTIVE", CanExecute = "PixiEditor.Layer.ReferenceLayerExists",
        IconEvaluator = "PixiEditor.Layer.ToggleReferenceLayerTopMostIcon", AnalyticsTrack = true)]
    public void ToggleReferenceLayerTopMost()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.ReferenceLayerViewModel.IsTopMost = !doc.ReferenceLayerViewModel.IsTopMost;
    }

    [Command.Basic("PixiEditor.Layer.ResetReferenceLayerPosition", "RESET_REFERENCE_LAYER_POS",
        "RESET_REFERENCE_LAYER_POS", CanExecute = "PixiEditor.Layer.ReferenceLayerExists",
        Icon = PixiPerfectIcons.Reset, AnalyticsTrack = true)]
    public void ResetReferenceLayerPosition()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        doc.Operations.ResetReferenceLayerPosition();
    }

    [Command.Basic("PixiEditor.Layer.Rasterize", "RASTERIZE_ACTIVE_LAYER", "RASTERIZE_ACTIVE_LAYER_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.AnySelectedLayerIsRasterizable",
        Icon = PixiPerfectIcons.LowresCircle, MenuItemPath = "LAYER/RASTERIZE_ACTIVE_LAYER",
        AnalyticsTrack = true)]
    public void RasterizeActiveLayer()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var selectedMembers = doc.ExtractSelectedLayers();
        if (selectedMembers.Count == 0)
            return;

        var block = doc.Operations.StartChangeBlock();

        foreach (var member in selectedMembers)
        {
            doc!.Operations.Rasterize(member);
        }

        block.Dispose();
    }


    [Command.Basic("PixiEditor.Layer.ConvertToCurve", "CONVERT_TO_CURVE", "CONVERT_TO_CURVE_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.AnySelectedMemberIsVectorLayer",
        MenuItemPath = "LAYER/CONVERT_TO_CURVE", AnalyticsTrack = true)]
    public void ConvertActiveLayersToCurve()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var selectedMembers = doc.ExtractSelectedLayers();
        if (selectedMembers.Count == 0)
            return;

        var block = doc.Operations.StartChangeBlock();

        foreach (var member in selectedMembers)
        {
            doc!.Operations.ConvertToCurve(member);
        }

        block.Dispose();
    }

    [Command.Basic("PixiEditor.Layer.SeparateShapes", "SEPARATE_SHAPES", "SEPARATE_SHAPES_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.AnySelectedMemberIsVectorLayer",
        MenuItemPath = "LAYER/SEPARATE_SHAPES", AnalyticsTrack = true)]
    public void SeparateShapes()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var selectedMembers = doc.ExtractSelectedLayers();
        if (selectedMembers.Count == 0)
            return;

        var block = doc.Operations.StartChangeBlock();

        foreach (var member in selectedMembers)
        {
            doc!.Operations.SeparateShapes(member);
        }

        block.Dispose();
    }

    [Command.Basic("PixiEditor.Layer.ExtractSelectedText", "EXTRACT_SELECTED_TEXT", "EXTRACT_SELECTED_TEXT_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.SelectedMemberIsSelectedText",
        Key = Key.X, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Parameter = false,
        ShortcutContexts = [typeof(ViewportWindowViewModel), typeof(TextOverlay)],
        MenuItemPath = "LAYER/EXTRACT_SELECTED_TEXT", AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Layer.ExtractSelectedCharacters", "EXTRACT_SELECTED_CHARACTERS",
        "EXTRACT_SELECTED_CHARACTERS_DESCRIPTIVE",
        CanExecute = "PixiEditor.Layer.SelectedMemberIsSelectedText",
        Key = Key.X, Modifiers = KeyModifiers.Control | KeyModifiers.Shift | KeyModifiers.Alt,
        Parameter = true,
        ShortcutContexts = [typeof(ViewportWindowViewModel), typeof(TextOverlay)],
        MenuItemPath = "LAYER/EXTRACT_SELECTED_CHARACTERS", AnalyticsTrack = true)]
    public void ExtractSelectedText(bool extractEachCharacter)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        var member = doc?.SelectedStructureMember;
        if (member is null)
            return;

        if (member is not VectorLayerNodeViewModel vectorLayer)
            return;

        int startIndex = doc.TextOverlayViewModel.CursorPosition;
        int endIndex = doc.TextOverlayViewModel.SelectionEnd;

        doc!.Operations.ExtractSelectedText(vectorLayer.Id, startIndex, endIndex, extractEachCharacter);
    }

    [Evaluator.Icon("PixiEditor.Layer.ToggleReferenceLayerTopMostIcon")]
    public IImage GetAboveEverythingReferenceLayerIcon()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || doc.ReferenceLayerViewModel.IsTopMost)
        {
            return PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.LayersTop);
        }

        return PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.LayersBottom, 18, 180);
    }
}
