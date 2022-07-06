using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.ViewModels.SubViewModels.Document;
using SkiaSharp;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentUpdater
{
    private DocumentViewModel doc;
    private DocumentHelpers helper;

    public DocumentUpdater(DocumentViewModel doc, DocumentHelpers helper)
    {
        this.doc = doc;
        this.helper = helper;
    }

    /// <summary>
    /// Don't call this outside ActionAccumulator
    /// </summary>
    public void ApplyChangeFromChangeInfo(IChangeInfo? arbitraryInfo)
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
            case CreateReferenceLayer_ChangeInfo info:
                ProcessCreateReferenceLayer(info);
                break;
            case SetSelectedMember_PassthroughAction info:
                ProcessSetSelectedMember(info);
                break;
            case AddSoftSelectedMember_PassthroughAction info:
                ProcessAddSoftSelectedMember(info);
                break;
            case ClearSoftSelectedMembers_PassthroughAction info:
                ProcessClearSoftSelectedMembers(info);
                break;
        }
    }

    private void ProcessClearSoftSelectedMembers(ClearSoftSelectedMembers_PassthroughAction info)
    {
        foreach (var oldMember in doc.SoftSelectedStructureMembers)
        {
            oldMember.IsSoftSelected = false;
            oldMember.RaisePropertyChanged(nameof(oldMember.IsSoftSelected));
        }
        doc.InternalClearSoftSelectedMembers();
    }

    private void ProcessAddSoftSelectedMember(AddSoftSelectedMember_PassthroughAction info)
    {
        var member = helper.StructureHelper.FindOrThrow(info.GuidValue);
        member.IsSoftSelected = true;
        member.RaisePropertyChanged(nameof(member.IsSoftSelected));
        doc.InternalAddSoftSelectedMember(member);
    }

    private void ProcessSetSelectedMember(SetSelectedMember_PassthroughAction info)
    {
        if (doc.SelectedStructureMember is { } oldMember)
        {
            oldMember.IsSelected = false;
            oldMember.RaisePropertyChanged(nameof(oldMember.IsSelected));
        }
        var member = helper.StructureHelper.FindOrThrow(info.GuidValue);
        member.IsSelected = true;
        member.RaisePropertyChanged(nameof(member.IsSelected));
        doc.InternalSetSelectedMember(member);
    }

    private void ProcessCreateReferenceLayer(CreateReferenceLayer_ChangeInfo info)
    {
        doc.RaisePropertyChanged(nameof(doc.ReferenceLayer));
        doc.RaisePropertyChanged(nameof(doc.ReferenceBitmap));
        doc.RaisePropertyChanged(nameof(doc.ReferenceBitmapSize));
        doc.RaisePropertyChanged(nameof(doc.ReferenceTransformMatrix));
        doc.RaisePropertyChanged(nameof(doc.ReferenceShape));
    }

    private void ProcessMaskIsVisible(StructureMemberMaskIsVisible_ChangeInfo info)
    {
        var member = helper.StructureHelper.FindOrThrow(info.GuidValue);
        member.SetMaskIsVisible(info.IsVisible);
    }

    private void ProcessClipToMemberBelow(StructureMemberClipToMemberBelow_ChangeInfo info)
    {
        var member = helper.StructureHelper.FindOrThrow(info.GuidValue);
        member.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
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
        var layer = (LayerViewModel)helper.StructureHelper.FindOrThrow(info.GuidValue);
        layer.SetLockTransparency(info.LockTransparency);
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        var memberVm = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.SetBlendMode(info.BlendMode);
    }

    private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
    {
        var memberVm = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.MaskPreviewSurface?.Dispose();
        memberVm.MaskPreviewSurface = null;
        memberVm.MaskPreviewBitmap = null;

        if (info.HasMask)
        {
            var size = StructureMemberViewModel.CalculatePreviewSize(doc.SizeBindable);
            memberVm.MaskPreviewBitmap = CreateBitmap(size);
            memberVm.MaskPreviewSurface = CreateSKSurface(memberVm.MaskPreviewBitmap);
        }
        memberVm.SetHasMask(info.HasMask);
        memberVm.RaisePropertyChanged(nameof(memberVm.MaskPreviewBitmap));
    }

    private void ProcessRefreshViewport(RefreshViewport_PassthroughAction info)
    {
        helper.State.Viewports[info.Info.GuidValue] = info.Info;
    }

    private void ProcessRemoveViewport(RemoveViewport_PassthroughAction info)
    {
        helper.State.Viewports.Remove(info.GuidValue);
    }

    private void UpdateMemberBitmapsRecursively(FolderViewModel folder, VecI newSize)
    {
        foreach (var member in folder.Children)
        {
            member.PreviewSurface.Dispose();
            member.PreviewBitmap = CreateBitmap(newSize);
            member.PreviewSurface = CreateSKSurface(member.PreviewBitmap);
            member.RaisePropertyChanged(nameof(member.PreviewBitmap));

            member.MaskPreviewSurface?.Dispose();
            member.MaskPreviewSurface = null;
            member.MaskPreviewBitmap = null;
            if (member.HasMaskBindable)
            {
                member.MaskPreviewBitmap = CreateBitmap(newSize);
                member.MaskPreviewSurface = CreateSKSurface(member.MaskPreviewBitmap);
            }
            member.RaisePropertyChanged(nameof(member.MaskPreviewBitmap));

            if (member is FolderViewModel innerFolder)
            {
                UpdateMemberBitmapsRecursively(innerFolder, newSize);
            }
        }
    }

    private void ProcessSize(Size_ChangeInfo info)
    {
        Dictionary<ChunkResolution, WriteableBitmap> newBitmaps = new();
        foreach (var (res, surf) in doc.Surfaces)
        {
            surf.Dispose();
            newBitmaps[res] = CreateBitmap((VecI)(info.Size * res.Multiplier()));
            doc.Surfaces[res] = CreateSKSurface(newBitmaps[res]);
        }

        doc.Bitmaps = newBitmaps;

        doc.InternalSetSize(info.Size);
        doc.InternalSetVerticalSymmetryAxisX(info.VerticalSymmetryAxisX);
        doc.InternalSetHorizontalSymmetryAxisY(info.HorizontalSymmetryAxisY);

        var previewSize = StructureMemberViewModel.CalculatePreviewSize(info.Size);
        doc.PreviewSurface.Dispose();
        doc.PreviewBitmap = CreateBitmap(previewSize);
        doc.PreviewSurface = CreateSKSurface(doc.PreviewBitmap);

        doc.RaisePropertyChanged(nameof(doc.Bitmaps));
        doc.RaisePropertyChanged(nameof(doc.PreviewBitmap));

        UpdateMemberBitmapsRecursively(doc.StructureRoot, previewSize);
    }

    private WriteableBitmap CreateBitmap(VecI size)
    {
        return new WriteableBitmap(Math.Max(size.X, 1), Math.Max(size.Y, 1), 96, 96, PixelFormats.Pbgra32, null);
    }

    private SKSurface CreateSKSurface(WriteableBitmap bitmap)
    {
        return SKSurface.Create(
            new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
            bitmap.BackBuffer,
            bitmap.BackBufferStride);
    }

    private void ProcessCreateStructureMember(CreateStructureMember_ChangeInfo info)
    {
        var parentFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(info.ParentGuid);

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
        memberVM.SetOpacity(info.Opacity);
        memberVM.SetIsVisible(info.IsVisible);
        memberVM.SetClipToMemberBelowEnabled(info.ClipToMemberBelow);
        memberVM.SetName(info.Name);
        memberVM.SetHasMask(info.HasMask);
        memberVM.SetMaskIsVisible(info.MaskIsVisible);
        memberVM.SetBlendMode(info.BlendMode);

        if (info.HasMask)
        {
            var size = StructureMemberViewModel.CalculatePreviewSize(doc.SizeBindable);
            memberVM.MaskPreviewBitmap = CreateBitmap(size);
            memberVM.MaskPreviewSurface = CreateSKSurface(memberVM.MaskPreviewBitmap);
        }

        parentFolderVM.Children.Insert(info.Index, memberVM);

        if (info is CreateFolder_ChangeInfo folderInfo)
        {
            foreach (CreateStructureMember_ChangeInfo childInfo in folderInfo.Children)
            {
                ProcessCreateStructureMember(childInfo);
            }
        }
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        var (memberVM, folderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
        folderVM.Children.Remove(memberVM);
        if (doc.SelectedStructureMember == memberVM)
            doc.InternalSetSelectedMember(null);
        doc.InternalClearSoftSelectedMembers();
    }

    private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetIsVisible(info.IsVisible);
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetName(info.Name);
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.SetOpacity(info.Opacity);
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
        var (memberVM, curFolderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);

        var targetFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(info.ParentToGuid);

        curFolderVM.Children.Remove(memberVM);
        targetFolderVM.Children.Insert(info.NewIndex, memberVM);
    }
}
