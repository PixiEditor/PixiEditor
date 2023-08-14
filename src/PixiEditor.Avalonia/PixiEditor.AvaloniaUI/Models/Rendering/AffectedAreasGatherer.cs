using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Rendering;
#nullable enable
internal class AffectedAreasGatherer
{
    private readonly DocumentChangeTracker tracker;

    public AffectedArea MainImageArea { get; private set; } = new();
    public Dictionary<Guid, AffectedArea> ImagePreviewAreas { get; private set; } = new();
    public Dictionary<Guid, AffectedArea> MaskPreviewAreas { get; private set; } = new();

    public AffectedAreasGatherer(DocumentChangeTracker tracker, IReadOnlyList<IChangeInfo> changes)
    {
        this.tracker = tracker;
        ProcessChanges(changes);
    }

    private void ProcessChanges(IReadOnlyList<IChangeInfo> changes)
    {
        foreach (var change in changes)
        {
            switch (change)
            {
                case MaskArea_ChangeInfo info:
                    if (info.Area.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Area);
                    AddToImagePreviews(info.GuidValue, info.Area, true);
                    AddToMaskPreview(info.GuidValue, info.Area);
                    break;
                case LayerImageArea_ChangeInfo info:
                    if (info.Area.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Area);
                    AddToImagePreviews(info.GuidValue, info.Area);
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
                case Size_ChangeInfo:
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
                case StructureMemberMaskIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.GuidValue, false);
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
            AddToImagePreviews(memberGuid, new AffectedArea(chunks), ignoreSelf);
        }
        else if (member is IReadOnlyFolder folder)
        {
            AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
            foreach (var child in folder.Children)
                AddAllToImagePreviews(child.GuidValue);
        }
    }

    private void AddAllToMainImage(Guid memberGuid, bool useMask = true)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyLayer layer)
        {
            var chunks = layer.LayerImage.FindAllChunks();
            if (layer.Mask is not null && layer.MaskIsVisible && useMask)
                chunks.IntersectWith(layer.Mask.FindAllChunks());
            AddToMainImage(new AffectedArea(chunks));
        }
        else
        {
            AddWholeCanvasToMainImage();
        }
    }

    private void AddAllToMaskPreview(Guid memberGuid)
    {
        if (!tracker.Document.TryFindMember(memberGuid, out var member))
            return;
        if (member.Mask is not null)
        {
            var chunks = member.Mask.FindAllChunks();
            AddToMaskPreview(memberGuid, new AffectedArea(chunks));
        }
        if (member is IReadOnlyFolder folder)
        {
            foreach (var child in folder.Children)
                AddAllToMaskPreview(child.GuidValue);
        }
    }


    private void AddToMainImage(AffectedArea area)
    {
        var temp = MainImageArea;
        temp.UnionWith(area);
        MainImageArea = temp;
    }

    private void AddToImagePreviews(Guid memberGuid, AffectedArea area, bool ignoreSelf = false)
    {
        var path = tracker.Document.FindMemberPath(memberGuid);
        if (path.Count < 2)
            return;
        for (int i = ignoreSelf ? 1 : 0; i < path.Count - 1; i++)
        {
            var member = path[i];
            if (!ImagePreviewAreas.ContainsKey(member.GuidValue))
            {
                ImagePreviewAreas[member.GuidValue] = new AffectedArea(area);
            }
            else
            {
                var temp = ImagePreviewAreas[member.GuidValue];
                temp.UnionWith(area);
                ImagePreviewAreas[member.GuidValue] = temp;
            }
        }
    }

    private void AddToMaskPreview(Guid memberGuid, AffectedArea area)
    {
        if (!MaskPreviewAreas.ContainsKey(memberGuid))
        {
            MaskPreviewAreas[memberGuid] = new AffectedArea(area);
        }
        else
        {
            var temp = MaskPreviewAreas[memberGuid];
            temp.UnionWith(area);
            MaskPreviewAreas[memberGuid] = temp;
        }
    }


    private void AddWholeCanvasToMainImage()
    {
        MainImageArea = AddWholeArea(MainImageArea);
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
            if (!ImagePreviewAreas.ContainsKey(member.GuidValue))
                ImagePreviewAreas[member.GuidValue] = new AffectedArea();
            ImagePreviewAreas[member.GuidValue] = AddWholeArea(ImagePreviewAreas[member.GuidValue]);
        }
    }

    private void AddWholeCanvasToMaskPreview(Guid memberGuid)
    {
        if (!MaskPreviewAreas.ContainsKey(memberGuid))
            MaskPreviewAreas[memberGuid] = new AffectedArea();
        MaskPreviewAreas[memberGuid] = AddWholeArea(MaskPreviewAreas[memberGuid]);
    }


    private void AddWholeCanvasToEveryImagePreview()
    {
        tracker.Document.ForEveryReadonlyMember((member) => AddWholeCanvasToImagePreviews(member.GuidValue));
    }

    private void AddWholeCanvasToEveryMaskPreview()
    {
        tracker.Document.ForEveryReadonlyMember((member) => 
        {
            if (member.Mask is not null)
                AddWholeCanvasToMaskPreview(member.GuidValue);
        });
    }

    private AffectedArea AddWholeArea(AffectedArea area)
    {
        VecI size = new(
            (int)Math.Ceiling(tracker.Document.Size.X / (float)ChunkyImage.FullChunkSize),
            (int)Math.Ceiling(tracker.Document.Size.Y / (float)ChunkyImage.FullChunkSize));
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                area.Chunks.Add(new(i, j));
            }
        }
        area.GlobalArea = new RectI(VecI.Zero, tracker.Document.Size);
        return area;
    }
}
