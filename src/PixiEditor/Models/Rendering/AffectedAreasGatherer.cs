using System.Collections.Generic;
using System.Diagnostics;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.ChangeInfos.Properties;
using PixiEditor.ChangeableDocument.ChangeInfos.Root;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.DocumentPassthroughActions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;

namespace PixiEditor.Models.Rendering;
#nullable enable
internal class AffectedAreasGatherer
{
    private readonly DocumentChangeTracker tracker;

    public AffectedArea MainImageArea { get; private set; } = new();
    public HashSet<Guid> ChangedMembers { get; private set; } = new();
    public bool IgnoreAnimationPreviews { get; private set; }
    public HashSet<Guid> ChangedMasks { get; private set; } = new();
    public HashSet<Guid> ChangedKeyFrames { get; private set; } = new();


    private KeyFrameTime ActiveFrame { get; set; }
    public HashSet<Guid> ChangedNodes { get; set; } = new();

    private bool alreadyAddedWholeCanvasToEveryImagePreview = false;

    public AffectedAreasGatherer(KeyFrameTime activeFrame, DocumentChangeTracker tracker,
        IReadOnlyList<IChangeInfo> changes, bool refreshAllPreviews)
    {
        this.tracker = tracker;
        ActiveFrame = activeFrame;

        if (refreshAllPreviews)
        {
            AddWholeCanvasToMainImage();
            AddWholeCanvasToEveryImagePreview(false);
            AddWholeCanvasToEveryMaskPreview();
            AddAllNodesToImagePreviews();
            AddAllKeyFrames();
            return;
        }

        ProcessChanges(changes);

        var outputNode = tracker.Document.NodeGraph.OutputNode;
        if (outputNode is null)
            return;

        if (tracker.Document.NodeGraph.CalculateExecutionQueue(tracker.Document.NodeGraph.OutputNode)
            .Any(x => x is ICustomShaderNode))
        {
            AddWholeCanvasToMainImage(); // Detecting what pixels shader might affect is very hard, so we just redraw everything
        }
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
                    AddToImagePreviews(info.Id, true);
                    AddToMaskPreview(info.Id);
                    AddToNodePreviews(info.Id);
                    break;
                case LayerImageArea_ChangeInfo info:
                    if (info.Area.Chunks is null)
                        throw new InvalidOperationException("Chunks must not be null");
                    AddToMainImage(info.Area);
                    AddToImagePreviews(info.Id);
                    AddToNodePreviews(info.Id);
                    break;
                case TransformObject_ChangeInfo info:
                    AddToMainImage(info.Area);
                    AddToImagePreviews(info.NodeGuid);
                    AddToNodePreviews(info.NodeGuid);
                    break;
                case CreateStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.Id, 0);
                    AddAllToImagePreviews(info.Id, 0);
                    AddAllToMaskPreview(info.Id);
                    AddToNodePreviews(info.Id);
                    break;
                case DeleteStructureMember_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToImagePreviews(info
                        .Id); // TODO: ParentGuid was here, make sure previews are updated correctly
                    break;
                case MoveStructureMember_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    if (info.ParentFromGuid != info.ParentToGuid)
                        AddWholeCanvasToImagePreviews(info.ParentFromGuid);
                    break;
                case Size_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(false);
                    AddWholeCanvasToEveryMaskPreview();
                    break;
                case StructureMemberMask_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToImagePreviews(info.Id, true);
                    AddToMaskPreview(info.Id);
                    AddToNodePreviews(info.Id);
                    break;
                case StructureMemberBlendMode_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    AddToNodePreviews(info.Id);
                    break;
                case StructureMemberClipToMemberBelow_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    AddToNodePreviews(info.Id);
                    break;
                case StructureMemberOpacity_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    AddToNodePreviews(info.Id);
                    break;
                case StructureMemberIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    AddToNodePreviews(info.Id);
                    break;
                case StructureMemberMaskIsVisible_ChangeInfo info:
                    AddAllToMainImage(info.Id, ActiveFrame, false);
                    AddAllToImagePreviews(info.Id, ActiveFrame, true);
                    AddToNodePreviews(info.Id);
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

                    AddKeyFrame(info.KeyFrameId);
                    break;
                case SetActiveFrame_PassthroughAction:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(true);
                    AddAllNodesToImagePreviews();
                    IgnoreAnimationPreviews = true;
                    break;
                case KeyFrameLength_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(true);
                    break;
                case DeleteKeyFrame_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(true);
                    break;
                case KeyFrameVisibility_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(true);
                    break;
                case ConnectProperty_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(false);
                    AddToNodePreviews(info.InputNodeId);
                    if (info.OutputNodeId.HasValue)
                    {
                        AddToNodePreviews(info.OutputNodeId.Value);
                    }

                    break;
                case PropertyValueUpdated_ChangeInfo info:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(false);
                    AddToNodePreviews(info.NodeId);
                    break;
                case ToggleOnionSkinning_PassthroughAction:
                    AddWholeCanvasToMainImage();
                    break;
                case OnionFrames_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    break;
                case VectorShape_ChangeInfo info:
                    AddToMainImage(info.Affected);
                    AddToImagePreviews(info.LayerId);
                    AddToNodePreviews(info.LayerId);
                    break;
                case ProcessingColorSpace_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(false);
                    AddWholeCanvasToEveryMaskPreview();
                    break;
                case RefreshPreview_PassthroughAction info:
                    ProcessRefreshPreview(info);
                    break;
                case BlackboardVariable_ChangeInfo or BlackboardVariableRemoved_ChangeInfo or RenameBlackboardVariable_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(false);
                    AddAllNodesToImagePreviews();
                    break;
                case FallbackAnimationToLayerImage_ChangeInfo:
                    AddWholeCanvasToMainImage();
                    AddWholeCanvasToEveryImagePreview(true);
                    AddAllNodesToImagePreviews();
                    break;
            }
        }
    }

    private void ProcessRefreshPreview(RefreshPreview_PassthroughAction info)
    {
        if (info.SubId == null)
        {
            if (info.ElementToRender == nameof(StructureNode.EmbeddedMask))
            {
                AddToMaskPreview(info.Id);
            }
            else
            {
                AddToImagePreviews(info.Id);
                AddToNodePreviews(info.Id);
            }
        }
        else
        {
            AddKeyFrame(info.SubId.Value);
        }
    }

    private void AddAllKeyFrames()
    {
        ChangedKeyFrames ??= new HashSet<Guid>();
        tracker.Document.ForEveryReadonlyMember((member) =>
        {
            foreach (var keyFrame in member.KeyFrames)
            {
                ChangedKeyFrames.Add(keyFrame.KeyFrameGuid);
            }

            ChangedKeyFrames.Add(member.Id);
        });
    }

    private void AddKeyFrame(Guid infoKeyFrameId)
    {
        ChangedKeyFrames ??= new HashSet<Guid>();
        ChangedKeyFrames.Add(infoKeyFrameId);
    }

    private void AddToNodePreviews(Guid nodeId)
    {
        ChangedNodes ??= new HashSet<Guid>();
        if (!ChangedNodes.Contains(nodeId))
        {
            ChangedNodes.Add(nodeId);
        }
    }

    private void AddAllNodesToImagePreviews()
    {
        foreach (var node in tracker.Document.NodeGraph.AllNodes)
        {
            AddToNodePreviews(node.Id);
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

            AddToImagePreviews(member, ignoreSelf);
        }
        else if (member is IReadOnlyFolderNode folder)
        {
            AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
            /*foreach (var child in folder.Children)
                AddAllToImagePreviews(child.Id, frame);*/
        }
        else if (member is IReadOnlyLayerNode genericLayerNode)
        {
            var tightBounds = genericLayerNode.GetTightBounds(frame);
            if (tightBounds is not null)
            {
                AddToImagePreviews(member, ignoreSelf);
            }
            else
            {
                AddWholeCanvasToImagePreviews(memberGuid, ignoreSelf);
            }
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
            if (layer.EmbeddedMask is not null && layer.MaskIsVisible.Value && useMask)
                chunks.IntersectWith(layer.EmbeddedMask.FindAllChunks());
            AddToMainImage(new AffectedArea(chunks));
        }
        else if (member is IReadOnlyLayerNode genericLayer)
        {
            var tightBounds = genericLayer.GetTightBounds(frame);
            if (tightBounds is not null)
            {
                var affectedArea = new AffectedArea(
                    OperationHelper.FindChunksTouchingRectangle((RectI)tightBounds.Value, ChunkyImage.FullChunkSize));

                var lastArea = new AffectedArea(affectedArea);

                AddToMainImage(affectedArea);
            }
            else
            {
                AddWholeCanvasToMainImage();
            }
        }
        else if (member is IReadOnlyFolderNode folder)
        {
            AddWholeCanvasToMainImage();
            /*foreach (var child in folder.Children)
                AddAllToMainImage(child.Id, frame);*/
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
        if (member.EmbeddedMask is not null)
        {
            var chunks = member.EmbeddedMask.FindAllChunks();
            AddToMaskPreview(memberGuid);
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

    private void AddToImagePreviews(Guid memberGuid, bool ignoreSelf = false)
    {
        var sourceMember = tracker.Document.FindMember(memberGuid);
        if (sourceMember is null)
        {
            // If the member is not found, we cannot add it to previews
            return;
        }

        AddToImagePreviews(sourceMember, ignoreSelf);
    }

    private void AddToImagePreviews(IReadOnlyStructureNode sourceMember, bool ignoreSelf)
    {
        var path = tracker.Document.GetParents(sourceMember.Id);
        path.Insert(0, sourceMember);
        for (int i = ignoreSelf ? 1 : 0; i < path.Count; i++)
        {
            var member = path[i];
            if (member == null) continue;

            ChangedMembers.Add(member.Id);
        }
    }

    private void AddToMaskPreview(Guid memberGuid)
    {
        ChangedMasks.Add(memberGuid);
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
            if (member is null) continue;

            ChangedMembers.Add(member.Id);
        }
    }

    private void AddWholeCanvasToEveryImagePreview(bool onlyWithKeyFrames)
    {
        if (alreadyAddedWholeCanvasToEveryImagePreview)
            return;

        tracker.Document.ForEveryReadonlyMember((member) =>
        {
            if (!onlyWithKeyFrames || member.KeyFrames.Count > 0)
            {
                AddWholeCanvasToImagePreviews(member.Id);
            }
        });
        alreadyAddedWholeCanvasToEveryImagePreview = true;
    }

    private void AddWholeCanvasToEveryMaskPreview()
    {
        tracker.Document.ForEveryReadonlyMember((member) =>
        {
            if (member.EmbeddedMask is not null)
            {
                ChangedMasks.Add(member.Id);
            }
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
