using System.Collections.Generic;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;
#nullable enable
internal class AffectedAreasGatherer
{
    private readonly DocumentChangeTracker tracker;

    public AffectedArea MainImageArea { get; private set; } = new();
    public Dictionary<Guid, AffectedArea> ImagePreviewAreas { get; private set; } = new();
    public Dictionary<Guid, AffectedArea> MaskPreviewAreas { get; private set; } = new();
    
    private KeyFrameTime ActiveFrame { get; set; }

    public AffectedAreasGatherer(KeyFrameTime activeFrame, DocumentChangeTracker tracker,
        IReadOnlyList<IChangeInfo> changes)
    {
        this.tracker = tracker;
        ActiveFrame = activeFrame;
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
                    AddToImagePreviews(info.Id, info.Area, true);
                    AddToMaskPreview(info.Id, info.Area);
                    break;
                case LayerImageArea_ChangeInfo info:
                    if (info.Area.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Area);
                    AddToImagePreviews(info.Id, info.Area);
                    break;
                case CreateStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.Id, 0);
                    AddAllToImagePreviews(info.Id, 0);
                    AddAllToMaskPreview(info.Id);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToImagePreviews(info.Id); // TODO: ParentGuid was here, make sure previews are updated correctly
                    break;
                case MoveStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
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
                    AddWholeCanvasToMaskPreview(info.Id);
                    AddWholeCanvasToImagePreviews(info.Id, true);
                    break;
                case StructureMemberBlendMode_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    break;
                case StructureMemberClipToMemberBelow_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    break;
                case StructureMemberMaskIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame, false);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    break;
                case CreateRasterKeyFrame_ChangeInfo info:
                    if (info.CloneFromExisting)
                    {
                        AddAllToMainImage(info.TargetLayerGuid, info.Frame);
                        AddAllToImagePreviews(info.TargetLayerGuid, info.Frame);
                    }
                    else
                    {
                        AddWholeCanvasToMainImage();
                        AddWholeCanvasToImagePreviews(info.TargetLayerGuid);
                    }
                    break;
                case SetActiveFrame_PassthroughAction:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
                case KeyFrameLength_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
                case DeleteKeyFrame_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
                case KeyFrameVisibility_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
                case ConnectProperty_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
                case PropertyValueUpdated_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview();
                    break;
            }
        }
    }

    private void AddAllToImagePreviews(Guid memberGuid, KeyFrameTime frame, bool ignoreSelf = false)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyImageNode layer)
        {
            var result = layer.GetLayerImageAtFrame(frame.Frame);
            if (result == null)
            {
                AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
                return;
            }
            
            var chunks = result.FindAllChunks();
            AddToImagePreviews(memberGuid, new AffectedArea(chunks), ignoreSelf);
        }
        else if (member is IReadOnlyFolderNode folder)
        {
            AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
            /*foreach (var child in folder.Children)
                AddAllToImagePreviews(child.Id, frame);*/
        }
    }

    private void AddAllToMainImage(Guid memberGuid, KeyFrameTime frame, bool useMask = true)
    {
        var member = tracker.Document.FindMember(memberGuid);
        if (member is IReadOnlyImageNode layer)
        {
            var result = layer.GetLayerImageAtFrame(frame.Frame);
            if (result == null)
            {
                AddWholeCanvasToMainImage();
                return;
            }
            
            var chunks = result.FindAllChunks();
            if (layer.Mask.Value is not null && layer.MaskIsVisible.Value && useMask)
                chunks.IntersectWith(layer.Mask.Value.FindAllChunks());
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
        if (member.Mask.Value is not null)
        {
            var chunks = member.Mask.Value.FindAllChunks();
            AddToMaskPreview(memberGuid, new AffectedArea(chunks));
        }
        if (member is IReadOnlyFolderNode folder)
        {
            /*foreach (var child in folder.Children)
                AddAllToMaskPreview(child.Id);
        */
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
        int minCount = ignoreSelf ? 2 : 1;
        if (path.Count < minCount)
            return;
        for (int i = ignoreSelf ? 1 : 0; i < path.Count; i++)
        {
            var member = path[i];
            if (!ImagePreviewAreas.ContainsKey(member.Id))
            {
                ImagePreviewAreas[member.Id] = new AffectedArea(area);
            }
            else
            {
                var temp = ImagePreviewAreas[member.Id];
                temp.UnionWith(area);
                ImagePreviewAreas[member.Id] = temp;
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
        if (path.Count < 1 || path.Count == 1 && ignoreSelf)
            return;
        // skip root folder
        for (int i = ignoreSelf ? 1 : 0; i < path.Count; i++)
        {
            var member = path[i];
            if (!ImagePreviewAreas.ContainsKey(member.Id))
                ImagePreviewAreas[member.Id] = new AffectedArea();
            ImagePreviewAreas[member.Id] = AddWholeArea(ImagePreviewAreas[member.Id]);
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
        tracker.Document.ForEveryReadonlyMember((member) => AddWholeCanvasToImagePreviews(member.Id));
    }

    private void AddWholeCanvasToEveryMaskPreview()
    {
        tracker.Document.ForEveryReadonlyMember((member) => 
        {
            if (member.Mask.Value is not null)
                AddWholeCanvasToMaskPreview(member.Id);
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
