using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;

namespace PixiEditorPrototype.Models;

internal class DocumentUpdater
{
    private DocumentViewModel doc;
    private DocumentHelpers helper;
    public DocumentUpdater(DocumentViewModel doc, DocumentHelpers helper)
    {
        this.doc = doc;
        this.helper = helper;
    }

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
        }
    }

    private void ProcessSelection(Selection_ChangeInfo info)
    {
        doc.RaisePropertyChanged(nameof(doc.SelectionPath));
    }

    private void ProcessLayerLockTransparency(LayerLockTransparency_ChangeInfo info)
    {
        var layer = (LayerViewModel)helper.StructureHelper.FindOrThrow(info.GuidValue);
        layer.RaisePropertyChanged(nameof(layer.LockTransparency));
    }

    private void ProcessStructureMemberBlendMode(StructureMemberBlendMode_ChangeInfo info)
    {
        var memberVm = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.RaisePropertyChanged(nameof(memberVm.BlendMode));
    }

    private void ProcessStructureMemberMask(StructureMemberMask_ChangeInfo info)
    {
        var memberVm = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVm.RaisePropertyChanged(nameof(memberVm.HasMask));
    }

    private void ProcessRefreshViewport(RefreshViewport_PassthroughAction info)
    {
        helper.State.Viewports[info.Location.GuidValue] = info.Location;
    }

    private void ProcessRemoveViewport(RemoveViewport_PassthroughAction info)
    {
        helper.State.Viewports.Remove(info.GuidValue);
    }

    private void ProcessSize(Size_ChangeInfo info)
    {
        var size = helper.Tracker.Document.Size;
        Dictionary<ChunkResolution, WriteableBitmap> newBitmaps = new();
        foreach (var (res, surf) in doc.Surfaces)
        {
            surf.Dispose();
            newBitmaps[res] = CreateBitmap((Vector2i)(size * res.Multiplier()));
            doc.Surfaces[res] = CreateSKSurface(newBitmaps[res]);
        }

        doc.Bitmaps = newBitmaps;
        doc.RaisePropertyChanged(nameof(doc.Bitmaps));
        doc.RaisePropertyChanged(nameof(doc.Width));
        doc.RaisePropertyChanged(nameof(doc.Height));
    }

    private WriteableBitmap CreateBitmap(Vector2i size)
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
        var (member, parentFolder) = helper.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);
        var parentFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(parentFolder.GuidValue);

        int index = parentFolder.ReadOnlyChildren.IndexOf(member);

        StructureMemberViewModel memberVM = member switch
        {
            IReadOnlyLayer layer => new LayerViewModel(doc, helper, layer),
            IReadOnlyFolder folder => new FolderViewModel(doc, helper, folder),
            _ => throw new InvalidOperationException("Unsupposed member type")
        };

        parentFolderVM.Children.Insert(index, memberVM);

        if (member is IReadOnlyFolder folder2)
        {
            foreach (IReadOnlyStructureMember child in folder2.ReadOnlyChildren)
            {
                ProcessCreateStructureMember(new CreateStructureMember_ChangeInfo() { GuidValue = child.GuidValue });
            }
        }
    }

    private void ProcessDeleteStructureMember(DeleteStructureMember_ChangeInfo info)
    {
        var (memberVM, folderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
        folderVM.Children.Remove(memberVM);
    }

    private void ProcessUpdateStructureMemberIsVisible(StructureMemberIsVisible_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.RaisePropertyChanged(nameof(memberVM.IsVisible));
    }

    private void ProcessUpdateStructureMemberName(StructureMemberName_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.RaisePropertyChanged(nameof(memberVM.Name));
    }

    private void ProcessUpdateStructureMemberOpacity(StructureMemberOpacity_ChangeInfo info)
    {
        var memberVM = helper.StructureHelper.FindOrThrow(info.GuidValue);
        memberVM.RaisePropertyChanged(nameof(memberVM.Opacity));
    }

    private void ProcessMoveStructureMember(MoveStructureMember_ChangeInfo info)
    {
        var (memberVM, curFolderVM) = helper.StructureHelper.FindChildAndParentOrThrow(info.GuidValue);
        var (member, targetFolder) = helper.Tracker.Document.FindChildAndParentOrThrow(info.GuidValue);

        int index = targetFolder.ReadOnlyChildren.IndexOf(member);
        var targetFolderVM = (FolderViewModel)helper.StructureHelper.FindOrThrow(targetFolder.GuidValue);

        curFolderVM.Children.Remove(memberVM);
        targetFolderVM.Children.Insert(index, memberVM);
    }
}
