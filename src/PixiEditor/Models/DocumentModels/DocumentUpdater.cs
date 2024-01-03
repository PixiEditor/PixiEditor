using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentUpdater
{
    private DocumentViewModel doc;
    private DocumentInternalParts helper;

    public DocumentUpdater(DocumentViewModel doc, DocumentInternalParts helper)
    {
        this.doc = doc;
        this.helper = helper;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void AfterUndoBoundaryPassed()
    {
        doc.RaisePropertyChanged(nameof(doc.AllChangesSaved));
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void ApplyChangeFromChangeInfo(IChangeInfo arbitraryInfo)
    {
        if (arbitraryInfo is null)
            return;

        switch (arbitraryInfo)
        {
            case CreateStructureMember_ChangeInfo info:
                ProcessCreateStructureMember(info);
                break;
            case DeleteStructureMember_ChangeInfo info:
                ProcessDeleteStructureMember(info);
                break;
            case StructureMemberName_ChangeInfo info:
                ProcessUpdateStructureMemberName(info);
                break;
            case StructureMemberIsVisible_ChangeInfo info:
                ProcessUpdateStructureMemberIsVisible(info);
                break;
            case StructureMemberOpacity_ChangeInfo info:
                ProcessUpdateStructureMemberOpacity(info);
                break;
            case MoveStructureMember_ChangeInfo info:
                ProcessMoveStructureMember(info);
                break;
            case Size_ChangeInfo info:
                ProcessSize(info);
                break;
            case RefreshViewport_PassthroughAction info:
                ProcessRefreshViewport(info);
                break;
            case RemoveViewport_PassthroughAction info:
                ProcessRemoveViewport(info);
                break;
            case StructureMemberMask_ChangeInfo info:
                ProcessStructureMemberMask(info);
                break;
            case StructureMemberBlendMode_ChangeInfo info:
                ProcessStructureMemberBlendMode(info);
                break;
            case LayerLockTransparency_ChangeInfo info:
                ProcessLayerLockTransparency(info);
                break;
            case Selection_ChangeInfo info:
                ProcessSelection(info);
                break;
            case SymmetryAxisState_ChangeInfo info:
                ProcessSymmetryState(info);
                break;
            case SymmetryAxisPosition_ChangeInfo info:
                ProcessSymmetryPosition(info);
                break;
            case StructureMemberClipToMemberBelow_ChangeInfo info:
                ProcessClipToMemberBelow(info);
                break;
            case StructureMemberMaskIsVisible_ChangeInfo info:
                ProcessMaskIsVisible(info);
                break;
            case SetReferenceLayer_ChangeInfo info:
                ProcessSetReferenceLayer(info);
                break;
            case DeleteReferenceLayer_ChangeInfo info:
                ProcessDeleteReferenceLayer(info);
                break;
            case TransformReferenceLayer_ChangeInfo info:
                ProcessTransformReferenceLayer(info);
                break;
            case ReferenceLayerIsVisible_ChangeInfo info:
                ProcessReferenceLayerIsVisible(info);
                break;
            case ReferenceLayerTopMost_ChangeInfo info:
                ProcessReferenceLayerTopMost(info);
                break;
            case SetSelectedMember_PassthroughAction info:
                ProcessSetSelectedMember(info);
                break;
            case AddSoftSelectedMember_PassthroughAction info:
                ProcessAddSoftSelectedMember(info);
                break;
            case RemoveSoftSelectedMember_PassthroughAction info:
                ProcessRemoveSoftSelectedMember(info);
                break;
            case ClearSoftSelectedMembers_PassthroughAction info:
                ProcessClearSoftSelectedMembers(info);
                break;
                
        }
    }

    private void ProcessReferenceLayerIsVisible(ReferenceLayerIsVisible_ChangeInfo info)
    {
        doc.ReferenceLayerViewModel.InternalSetReferenceLayerIsVisible(info.IsVisible);
    }

    private void ProcessTransformReferenceLayer(TransformReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerViewModel.InternalTransformReferenceLayer(info.Corners);
    }

    private void ProcessDeleteReferenceLayer(DeleteReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerViewModel.InternalDeleteReferenceLayer();
    }

    private void ProcessSetReferenceLayer(SetReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerViewModel.InternalSetReferenceLayer(info.ImageRgba64Bytes, info.ImageSize, info.Shape);
    }
    
    private void ProcessReferenceLayerTopMost(ReferenceLayerTopMost_ChangeInfo info)
    {
        doc.ReferenceLayerViewModel.InternalSetReferenceLayerTopMost(info.IsTopMost);
    }

    private void ProcessRemoveSoftSelectedMember(RemoveSoftSelectedMember_PassthroughAction info)
    {
        StructureMemberViewModel? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        if (member.Selection != StructureMemberSelectionType.Soft)
            return;
        member.Selection = StructureMemberSelectionType.None;
        member.RaisePropertyChanged(nameof(member.Selection));
        doc.InternalRemoveSoftSelectedMember(member);
    }

    private void ProcessClearSoftSelectedMembers(ClearSoftSelectedMembers_PassthroughAction info)
    {
        foreach (StructureMemberViewModel? oldMember in doc.SoftSelectedStructureMembers)
        {
            if (oldMember.Selection == StructureMemberSelectionType.Hard)
                continue;
            oldMember.Selection = StructureMemberSelectionType.None;
            oldMember.RaisePropertyChanged(nameof(oldMember.Selection));
        }
        doc.InternalClearSoftSelectedMembers();
    }

    private void ProcessAddSoftSelectedMember(AddSoftSelectedMember_PassthroughAction info)
    {
        StructureMemberViewModel? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        member.Selection = StructureMemberSelectionType.Soft;
        member.RaisePropertyChanged(nameof(member.Selection));
        doc.InternalAddSoftSelectedMember(member);
    }

    private void ProcessSetSelectedMember(SetSelectedMember_PassthroughAction info)
    {
        StructureMemberViewModel? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        if (doc.SelectedStructureMember is { } oldMember)
        {
            oldMember.Selection = StructureMemberSelectionType.None;
            oldMember.RaisePropertyChanged(nameof(oldMember.Selection));
        }
        member.Selection = StructureMemberSelectionType.Hard;
        member.RaisePropertyChanged(nameof(member.Selection));
        doc.InternalSetSelectedMember(member);
    }

    private void ProcessMaskIsVisible(StructureMemberMaskIsVisible_ChangeInfo info)
    {
        StructureMemberViewModel? member = doc.StructureHelper.FindOrThrow(info.GuidValue);
        member.InternalSetMaskIsVisible(info.IsVisible);
    }

    private void ProcessClipToMemberBelow(StructureMemberClipToMemberBelow_ChangeInfo info)
    {
        StructureMemberViewModel? member = doc.StructureHelper.FindOrThrow(info.GuidValue);
        member.InternalSetClipToMemberBelowEnabled(info.ClipToMemberBelow);
    }

    private void ProcessSymmetryPosition(SymmetryAxisPosition_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.InternalSetHorizontalSymmetryAxisY(info.NewPosition);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.InternalSetVerticalSymmetryAxisX(info.NewPosition);
    }

    private void ProcessSymmetryState(SymmetryAxisState_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.InternalSetHorizontalSymmetryAxisEnabled(info.State);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.InternalSetVerticalSymmetryAxisEnabled(info.State);
    }

    private void ProcessSelection(Selection_ChangeInfo info)
    {
        doc.InternalUpdateSelectionPath(info.NewPath);
    }

    private void ProcessLayerLockTransparency(LayerLockTransparency_ChangeInfo info)
    {
        LayerViewModel? layer = (LayerViewModel)doc.StructureHelper.FindOrThrow(info.GuidValue);
        layer.SetLockTransparency(info.LockTransparency);
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        StructureMemberViewModel? memberVm = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.InternalSetBlendMode(info.BlendMode);
    }

    private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
    {
        StructureMemberViewModel? memberVm = doc.StructureHelper.FindOrThrow(info.GuidValue);

        memberVm.InternalSetHasMask(info.HasMask);
        memberVm.RaisePropertyChanged(nameof(memberVm.MaskPreviewBitmap));
        if (!info.HasMask && memberVm is LayerViewModel layer)
            layer.ShouldDrawOnMask = false;
    }

    private void ProcessRefreshViewport(RefreshViewport_PassthroughAction info)
    {
        helper.State.Viewports[info.Info.GuidValue] = info.Info;
    }

    private void ProcessRemoveViewport(RemoveViewport_PassthroughAction info)
    {
        helper.State.Viewports.Remove(info.GuidValue);
    }

    private void ProcessSize(Size_ChangeInfo info)
    {
        VecI oldSize = doc.SizeBindable;

        Dictionary<ChunkResolution, WriteableBitmap> newBitmaps = new();
        foreach ((ChunkResolution res, DrawingSurface surf) in doc.Surfaces)
        {
            surf.Dispose();
            newBitmaps[res] = StructureMemberViewModel.CreateBitmap((VecI)(info.Size * res.Multiplier()));
            doc.Surfaces[res] = StructureMemberViewModel.CreateDrawingSurface(newBitmaps[res]);
        }

        doc.LazyBitmaps = newBitmaps;

        doc.InternalSetSize(info.Size);
        doc.InternalSetVerticalSymmetryAxisX(info.VerticalSymmetryAxisX);
        doc.InternalSetHorizontalSymmetryAxisY(info.HorizontalSymmetryAxisY);

        VecI documentPreviewSize = StructureMemberViewModel.CalculatePreviewSize(info.Size);
        doc.PreviewSurface.Dispose();
        doc.PreviewBitmap = StructureMemberViewModel.CreateBitmap(documentPreviewSize);
        doc.PreviewSurface = StructureMemberViewModel.CreateDrawingSurface(doc.PreviewBitmap);

        doc.RaisePropertyChanged(nameof(doc.LazyBitmaps));
        doc.RaisePropertyChanged(nameof(doc.PreviewBitmap));

        doc.InternalRaiseSizeChanged(new(doc, oldSize, info.Size));
    }

    private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
    {
        FolderViewModel? parentFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(info.ParentGuid);

        StructureMemberViewModel memberVM;
        if (info is CreateLayer_ChangeInfo layerInfo)
        {
            memberVM = new LayerViewModel(doc, helper, info.GuidValue);
            ((LayerViewModel)memberVM).SetLockTransparency(layerInfo.LockTransparency);
        }
        else if (info is CreateFolder_ChangeInfo)
        {
            memberVM = new FolderViewModel(doc, helper, info.GuidValue);
        }
        else
        {
            throw new NotSupportedException();
        }
        memberVM.InternalSetOpacity(info.Opacity);
        memberVM.InternalSetIsVisible(info.IsVisible);
        memberVM.InternalSetClipToMemberBelowEnabled(info.ClipToMemberBelow);
        memberVM.InternalSetName(info.Name);
        memberVM.InternalSetHasMask(info.HasMask);
        memberVM.InternalSetMaskIsVisible(info.MaskIsVisible);
        memberVM.InternalSetBlendMode(info.BlendMode);

        parentFolderVM.Children.Insert(info.Index, memberVM);

        if (info is CreateFolder_ChangeInfo folderInfo)
        {
            foreach (CreateStructureMember_ChangeInfo childInfo in folderInfo.Children)
            {
                ProcessCreateStructureMember(childInfo);
            }
        }
        
        if (doc.SelectedStructureMember is not null)
        {
            doc.SelectedStructureMember.Selection = StructureMemberSelectionType.None;
            doc.SelectedStructureMember.RaisePropertyChanged(nameof(doc.SelectedStructureMember.Selection));
        }
        
        doc.InternalSetSelectedMember(memberVM);
        memberVM.Selection = StructureMemberSelectionType.Hard;
        doc.RaisePropertyChanged(nameof(doc.SelectedStructureMember));
        doc.RaisePropertyChanged(nameof(memberVM.Selection));

        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Add));
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        (StructureMemberViewModel memberVM, FolderViewModel folderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
        folderVM.Children.Remove(memberVM);
        if (doc.SelectedStructureMember == memberVM)
            doc.InternalSetSelectedMember(null);
        doc.InternalClearSoftSelectedMembers();
        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Remove));
    }

    private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
    {
        StructureMemberViewModel? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.InternalSetIsVisible(info.IsVisible);
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        StructureMemberViewModel? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.InternalSetName(info.Name);
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        StructureMemberViewModel? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.InternalSetOpacity(info.Opacity);
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
        (StructureMemberViewModel memberVM, FolderViewModel curFolderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);

        FolderViewModel? targetFolderVM = (FolderViewModel)doc.StructureHelper.FindOrThrow(info.ParentToGuid);

        curFolderVM.Children.Remove(memberVM);
        targetFolderVM.Children.Insert(info.NewIndex, memberVM);
        doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Move));
    }
}
