using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;
#nullable enable
internal class DocumentUpdater
{
    private IDocument doc;
    private DocumentInternalParts helper;

    public DocumentUpdater(IDocument doc, DocumentInternalParts helper)
    {
        this.doc = doc;
        this.helper = helper;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void AfterUndoBoundaryPassed()
    {
        //TODO: Make sure AllChangesSaved trigger raise property changed itself
        doc.UpdateSavedState();
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void ApplyChangeFromChangeInfo(IChangeInfo arbitraryInfo)
    {
        if (arbitraryInfo is null)
            return;

        //TODO: Find a more elegant way to do this
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
            case CreateRasterKeyFrame_ChangeInfo info:
                ProcessCreateRasterKeyFrame(info);
                break;
            case DeleteKeyFrame_ChangeInfo info:
                ProcessDeleteKeyFrame(info);
                break;
            case SetActiveFrame_PassthroughAction info:
                ProcessActiveFrame(info);
                break;
            case KeyFrameLength_ChangeInfo info:
                ProcessKeyFrameLength(info);
                break;
            case KeyFrameVisibility_ChangeInfo info:
                ProcessKeyFrameVisibility(info);
                break;
            case AddSelectedKeyFrame_PassthroughAction info:
                ProcessAddSelectedKeyFrame(info);
                break;
            case RemoveSelectedKeyFrame_PassthroughAction info:
                ProcessRemoveSelectedKeyFrame(info);
                break;
            case ClearSelectedKeyFrames_PassthroughAction info:
                ClearSelectedKeyFrames(info);
                break;
        }
    }

    private void ProcessReferenceLayerIsVisible(ReferenceLayerIsVisible_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayerIsVisible(info.IsVisible);
    }

    private void ProcessTransformReferenceLayer(TransformReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.TransformReferenceLayer(info.Corners);
    }

    private void ProcessDeleteReferenceLayer(DeleteReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.DeleteReferenceLayer();
    }

    private void ProcessSetReferenceLayer(SetReferenceLayer_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayer(info.ImagePbgra8888Bytes, info.ImageSize, info.Shape);
    }
    
    private void ProcessReferenceLayerTopMost(ReferenceLayerTopMost_ChangeInfo info)
    {
        doc.ReferenceLayerHandler.SetReferenceLayerTopMost(info.IsTopMost);
    }

    private void ProcessRemoveSoftSelectedMember(RemoveSoftSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        if (member.Selection != StructureMemberSelectionType.Soft)
            return;
        member.Selection = StructureMemberSelectionType.None;
        // TODO: Make sure Selection raises property changed internally
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.RemoveSoftSelectedMember(member);
    }

    private void ProcessClearSoftSelectedMembers(ClearSoftSelectedMembers_PassthroughAction info)
    {
        foreach (IStructureMemberHandler? oldMember in doc.SoftSelectedStructureMembers)
        {
            if (oldMember.Selection == StructureMemberSelectionType.Hard)
                continue;
            oldMember.Selection = StructureMemberSelectionType.None;
            //oldMember.OnPropertyChanged(nameof(oldMember.Selection));
        }
        doc.ClearSoftSelectedMembers();
    }

    private void ProcessAddSoftSelectedMember(AddSoftSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        member.Selection = StructureMemberSelectionType.Soft;
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.AddSoftSelectedMember(member);
    }

    private void ProcessSetSelectedMember(SetSelectedMember_PassthroughAction info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.Find(info.GuidValue);
        if (member is null || member.Selection == StructureMemberSelectionType.Hard)
            return;
        
        if (doc.SelectedStructureMember is { } oldMember)
        {
            oldMember.Selection = StructureMemberSelectionType.None;
            //oldMember.OnPropertyChanged(nameof(oldMember.Selection));
        }
        member.Selection = StructureMemberSelectionType.Hard;
        //member.OnPropertyChanged(nameof(member.Selection));
        doc.SetSelectedMember(member);
    }

    private void ProcessMaskIsVisible(StructureMemberMaskIsVisible_ChangeInfo info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.FindOrThrow(info.GuidValue);
        member.SetMaskIsVisible(info.IsVisible);
    }

    private void ProcessClipToMemberBelow(StructureMemberClipToMemberBelow_ChangeInfo info)
    {
        IStructureMemberHandler? member = doc.StructureHelper.FindOrThrow(info.GuidValue);
        member.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
    }

    private void ProcessSymmetryPosition(SymmetryAxisPosition_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.SetHorizontalSymmetryAxisY(info.NewPosition);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.SetVerticalSymmetryAxisX(info.NewPosition);
    }

    private void ProcessSymmetryState(SymmetryAxisState_ChangeInfo info)
    {
        if (info.Direction == SymmetryAxisDirection.Horizontal)
            doc.SetHorizontalSymmetryAxisEnabled(info.State);
        else if (info.Direction == SymmetryAxisDirection.Vertical)
            doc.SetVerticalSymmetryAxisEnabled(info.State);
    }

    private void ProcessSelection(Selection_ChangeInfo info)
    {
        doc.UpdateSelectionPath(info.NewPath);
    }

    private void ProcessLayerLockTransparency(LayerLockTransparency_ChangeInfo info)
    {
        ILayerHandler? layer = (ILayerHandler)doc.StructureHelper.FindOrThrow(info.GuidValue);
        layer.SetLockTransparency(info.LockTransparency);
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        IStructureMemberHandler? memberVm = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.SetBlendMode(info.BlendMode);
    }

    private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
    {
        IStructureMemberHandler? memberVm = doc.StructureHelper.FindOrThrow(info.GuidValue);

        memberVm.SetHasMask(info.HasMask);
        // TODO: Make sure HasMask raises property changed internally
        //memberVm.OnPropertyChanged(nameof(memberVm.MaskPreviewBitmap));
        if (!info.HasMask && memberVm is ILayerHandler layer)
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

        foreach ((ChunkResolution res, Surface surf) in doc.Surfaces)
        {
            surf.Dispose();
            VecI size = (VecI)(info.Size * res.Multiplier());
            doc.Surfaces[res] = new Surface(new VecI(Math.Max(size.X, 1), Math.Max(size.Y, 1))); //TODO: Bgra8888 was here
        }

        doc.SetSize(info.Size);
        doc.SetVerticalSymmetryAxisX(info.VerticalSymmetryAxisX);
        doc.SetHorizontalSymmetryAxisY(info.HorizontalSymmetryAxisY);

        VecI documentPreviewSize = StructureHelpers.CalculatePreviewSize(info.Size);
        doc.PreviewSurface.Dispose();
        doc.PreviewSurface = new Surface(documentPreviewSize); //TODO: Bgra8888 was here

        // TODO: Make sure property changed events are raised internally
        // UPDATE: I think I did, but I'll leave it commented out for now
        /*doc.OnPropertyChanged(nameof(doc.LazyBitmaps));
        doc.OnPropertyChanged(nameof(doc.PreviewBitmap));
        doc.InternalRaiseSizeChanged(new DocumentSizeChangedEventArgs(doc, oldSize, info.Size));*/
    }

    private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
    {
        IFolderHandler? parentFolderVM = (IFolderHandler)doc.StructureHelper.FindOrThrow(info.ParentGuid);

        IStructureMemberHandler memberVM;
        if (info is CreateLayer_ChangeInfo layerInfo)
        {
            memberVM = doc.LayerHandlerFactory.CreateLayerHandler(helper, info.GuidValue);
            ((ILayerHandler)memberVM).SetLockTransparency(layerInfo.LockTransparency);
        }
        else if (info is CreateFolder_ChangeInfo)
        {
            memberVM = doc.FolderHandlerFactory.CreateFolderHandler(helper, info.GuidValue);
        }
        else
        {
            throw new NotSupportedException();
        }

        memberVM.SetOpacity(info.Opacity);
        memberVM.SetIsVisible(info.IsVisible);
        memberVM.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
        memberVM.SetName(info.Name);
        memberVM.SetHasMask(info.HasMask);
        memberVM.SetMaskIsVisible(info.MaskIsVisible);
        memberVM.SetBlendMode(info.BlendMode);

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
            // TODO: Make sure property changed events are raised internally
            //doc.SelectedStructureMember.OnPropertyChanged(nameof(doc.SelectedStructureMember.Selection));
        }

        doc.SetSelectedMember(memberVM);
        memberVM.Selection = StructureMemberSelectionType.Hard;

        // TODO: Make sure property changed events are raised internally
        /*doc.OnPropertyChanged(nameof(doc.SelectedStructureMember));
        doc.OnPropertyChanged(nameof(memberVM.Selection));*/

        //doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Add));
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        (IStructureMemberHandler memberVM, IFolderHandler folderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
        folderVM.Children.Remove(memberVM);
        if (doc.SelectedStructureMember == memberVM)
            doc.SetSelectedMember(null);
        doc.ClearSoftSelectedMembers();
        // TODO: Make sure property changed events are raised internally
        //doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Remove));
    }

    private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetIsVisible(info.IsVisible);
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetName(info.Name);
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        IStructureMemberHandler? memberVM = doc.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetOpacity(info.Opacity);
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
        (IStructureMemberHandler memberVM, IFolderHandler curFolderVM) = doc.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);

        IFolderHandler? targetFolderVM = (IFolderHandler)doc.StructureHelper.FindOrThrow(info.ParentToGuid);

        curFolderVM.Children.Remove(memberVM);
        targetFolderVM.Children.Insert(info.NewIndex, memberVM);

        // TODO: Make sure property changed events are raised internally
        //doc.InternalRaiseLayersChanged(new LayersChangedEventArgs(info.GuidValue, LayerAction.Move));
    }
    
    private void ProcessCreateRasterKeyFrame(CreateRasterKeyFrame_ChangeInfo info)
    {
        doc.AnimationHandler.AddKeyFrame(new RasterKeyFrameViewModel(info.TargetLayerGuid, info.Frame, 1, info.KeyFrameId, 
            (DocumentViewModel)doc, helper));
    }
    
    private void ProcessDeleteKeyFrame(DeleteKeyFrame_ChangeInfo info)
    {
        doc.AnimationHandler.RemoveKeyFrame(info.DeletedKeyFrameId);
    }
    
    private void ProcessActiveFrame(SetActiveFrame_PassthroughAction info)
    {
        doc.AnimationHandler.SetActiveFrame(info.Frame);
    }
    
    private void ProcessKeyFrameLength(KeyFrameLength_ChangeInfo info)
    {
        doc.AnimationHandler.SetFrameLength(info.KeyFrameGuid, info.StartFrame, info.Duration);
    }
    
    private void ProcessKeyFrameVisibility(KeyFrameVisibility_ChangeInfo info)
    {
        doc.AnimationHandler.SetKeyFrameVisibility(info.KeyFrameId, info.IsVisible);
    }
    
    private void ProcessAddSelectedKeyFrame(AddSelectedKeyFrame_PassthroughAction info)
    {
        doc.AnimationHandler.AddSelectedKeyFrame(info.KeyFrameGuid);
    }
    
    private void ProcessRemoveSelectedKeyFrame(RemoveSelectedKeyFrame_PassthroughAction info)
    {
        doc.AnimationHandler.RemoveSelectedKeyFrame(info.KeyFrameGuid);
    }
    
    private void ClearSelectedKeyFrames(ClearSelectedKeyFrames_PassthroughAction info)
    {
        doc.AnimationHandler.ClearSelectedKeyFrames();
    }
}
