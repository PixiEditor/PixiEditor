using System;
using System.Collections.Generic;
using ChangeableDocument;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;

namespace PixiEditorPrototype.Models.Rendering;
internal class AffectedChunkGatherer
{
    private readonly DocumentChangeTracker tracker;

    public HashSet<VecI> mainImageChunks { get; private set; } = new();
    public Dictionary<Guid, HashSet<VecI>> imagePreviewChunks { get; private set; } = new();
    public Dictionary<Guid, HashSet<VecI>> maskPreviewChunks { get; private set; } = new();

    public AffectedChunkGatherer(DocumentChangeTracker tracker, IReadOnlyList<IChangeInfo?> changes)
    {
        this.tracker = tracker;
        ProcessChanges(changes);
    }

    private void ProcessChanges(IReadOnlyList<IChangeInfo?> changes)
    {
        foreach (var change in changes)
        {
            switch (change)
            {
                case MaskChunks_ChangeInfo info:
                    if (info.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Chunks);
                    AddToImagePreviews(info.GuidValue, info.Chunks, true);
                    AddToMaskPreview(info.GuidValue, info.Chunks);
                    break;
                case LayerImageChunks_ChangeInfo info:
                    if (info.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Chunks);
                    AddToImagePreviews(info.GuidValue, info.Chunks);
                    break;
                case CreateStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue);
                    AddAllToMaskPreview(info.GuidValue);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToImagePreviews(info.ParentGuid);
                    break;
                case MoveStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue, true);
                    if (info.ParentFromGuid != info.ParentToGuid)
                        AddWholeCanvasToImagePreviews(info.ParentFromGuid);
                    break;
                case Size_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    AddWholeCanvasToEveryMaskPreview();
                    break;
                case StructureMemberMask_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToMaskPreview(info.GuidValue);
                    AddWholeCanvasToImagePreviews(info.GuidValue, true);
                    break;
                case StructureMemberBlendMode_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue, true);
                    break;
                case StructureMemberClipToMemberBelow_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue, true);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue, true);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue);
                    AddAllToImagePreviews(info.GuidValue, true);
                    break;
            }
        }
    }

    private void AddAllToImagePreviews(Guid memberGuid, bool ignoreSelf = false)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyLayer layer)
        {
            var chunks = layer.LayerImage.FindAllChunks();
            AddToImagePreviews(memberGuid, chunks, ignoreSelf);
        }
        else
        {
            AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
        }
    }
    private void AddAllToMainImage(Guid memberGuid)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyLayer layer)
        {
            var chunks = layer.LayerImage.FindAllChunks();
            if (layer.Mask is not null)
                chunks.IntersectWith(layer.Mask.FindAllChunks());
            AddToMainImage(chunks);
        }
        else
        {
            AddWholeCanvasToMainImage();
        }
    }
    private void AddAllToMaskPreview(Guid memberGuid)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is null || member.Mask is null)
            return;
        var chunks = member.Mask.FindAllChunks();
        AddToMaskPreview(memberGuid, chunks);
    }


    private void AddToMainImage(HashSet<VecI> chunks)
    {
        mainImageChunks.UnionWith(chunks);
    }
    private void AddToImagePreviews(Guid memberGuid, HashSet<VecI> chunks, bool ignoreSelf = false)
    {
        var path = tracker.Document.FindMemberPath(memberGuid);
        if (path.Count < 2)
            return;
        for (int i = ignoreSelf ? 1 : 0; i < path.Count - 1; i++)
        {
            var member = path[i];
            if (!imagePreviewChunks.ContainsKey(member.GuidValue))
                imagePreviewChunks[member.GuidValue] = new HashSet<VecI>(chunks);
            else
                imagePreviewChunks[member.GuidValue].UnionWith(chunks);
        }
    }
    private void AddToMaskPreview(Guid memberGuid, HashSet<VecI> chunks)
    {
        if (!maskPreviewChunks.ContainsKey(memberGuid))
            maskPreviewChunks[memberGuid] = new HashSet<VecI>(chunks);
        else
            maskPreviewChunks[memberGuid].UnionWith(chunks);
    }


    private void AddWholeCanvasToMainImage()
    {
        AddAllChunks(mainImageChunks);
    }
    private void AddWholeCanvasToImagePreviews(Guid memberGuid, bool ignoreSelf = false)
    {
        var path = tracker.Document.FindMemberPath(memberGuid);
        if (path.Count < 2)
            return;
        // skip root folder
        for (int i = ignoreSelf ? 1 : 0; i < path.Count - 1; i++)
        {
            var member = path[i];
            if (!imagePreviewChunks.ContainsKey(member.GuidValue))
                imagePreviewChunks[member.GuidValue] = new HashSet<VecI>();
            AddAllChunks(imagePreviewChunks[member.GuidValue]);
        }
    }
    private void AddWholeCanvasToMaskPreview(Guid memberGuid)
    {
        if (!maskPreviewChunks.ContainsKey(memberGuid))
            maskPreviewChunks[memberGuid] = new HashSet<VecI>();
        AddAllChunks(maskPreviewChunks[memberGuid]);
    }


    private void AddWholeCanvasToEveryImagePreview()
    {
        ForEveryMember(tracker.Document.StructureRoot, (member) => AddWholeCanvasToImagePreviews(member.GuidValue));
    }

    private void AddWholeCanvasToEveryMaskPreview()
    {
        ForEveryMember(tracker.Document.StructureRoot, (member) => AddWholeCanvasToMaskPreview(member.GuidValue));
    }


    private void ForEveryMember(IReadOnlyFolder folder, Action<IReadOnlyStructureMember> action)
    {
        foreach (var child in folder.Children)
        {
            action(child);
            if (child is IReadOnlyFolder innerFolder)
                ForEveryMember(innerFolder, action);
        }
    }

    private void AddAllChunks(HashSet<VecI> chunks)
    {
        VecI size = new(
            (int)Math.Ceiling(tracker.Document.Size.X / (float)ChunkyImage.ChunkSize),
            (int)Math.Ceiling(tracker.Document.Size.Y / (float)ChunkyImage.ChunkSize));
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                chunks.Add(new(i, j));
            }
        }
    }
}
