using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Structure;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class CombineStructureMembersOnto_Change : Change
{
    private HashSet<Guid> membersToMerge;

    private HashSet<Guid> layersToCombine = new();

    private Guid targetLayerGuid;
    private Dictionary<int, CommittedChunkStorage> originalChunks = new();

    private Dictionary<int, VectorPath> originalPaths = new();


    [GenerateMakeChangeAction]
    public CombineStructureMembersOnto_Change(HashSet<Guid> membersToMerge, Guid targetLayer)
    {
        this.membersToMerge = new HashSet<Guid>(membersToMerge);
        this.targetLayerGuid = targetLayer;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (!target.HasMember(targetLayerGuid) || membersToMerge.Count == 0)
            return false;
        foreach (Guid guid in membersToMerge)
        {
            if (!target.TryFindMember(guid, out var member))
                return false;

            AddMember(member);
        }

        return true;
    }

    private void AddMember(StructureNode member)
    {
        if (member is LayerNode layer)
        {
            layersToCombine.Add(layer.Id);
        }
        else if (member is FolderNode innerFolder)
        {
            layersToCombine.Add(innerFolder.Id);
            AddChildren(innerFolder, layersToCombine);
        }

        if (member is { ClipToPreviousMember: true, Background.Connection: not null })
        {
            if (member.Background.Connection.Node is StructureNode structureNode)
            {
                AddMember(structureNode);
            }
            else
            {
                member.Background.Connection.Node.TraverseBackwards(node =>
                {
                    if (node is StructureNode strNode)
                    {
                        layersToCombine.Add(strNode.Id);
                        return false;
                    }

                    return true;
                });
            }
        }
    }

    private void AddChildren(FolderNode folder, HashSet<Guid> collection)
    {
        if (folder.Content.Connection != null)
        {
            folder.Content.Connection.Node.TraverseBackwards(node =>
            {
                if (node is LayerNode layer)
                {
                    collection.Add(layer.Id);
                    return true;
                }

                return true;
            });
        }
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        List<IChangeInfo> changes = new();
        var targetLayer = target.FindMemberOrThrow<StructureNode>(targetLayerGuid);

        int maxFrame = GetMaxFrame(target, targetLayer);

        for (int frame = 0; frame < maxFrame || frame == 0; frame++)
        {
            changes.AddRange(ApplyToFrame(target, targetLayer, frame));
        }

        ignoreInUndo = false;

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var toDrawOn = target.FindMemberOrThrow<LayerNode>(targetLayerGuid);

        List<IChangeInfo> changes = new();

        int maxFrame = GetMaxFrame(target, toDrawOn);

        for (int frame = 0; frame < maxFrame || frame == 0; frame++)
        {
            changes.Add(RevertFrame(toDrawOn, frame));
        }

        target.AnimationData.RemoveKeyFrame(targetLayerGuid);
        originalChunks.Clear();
        changes.Add(new DeleteKeyFrame_ChangeInfo(targetLayerGuid));

        return changes;
    }

    private List<IChangeInfo> ApplyToFrame(Document target, StructureNode targetLayer, int frame)
    {
        var chunksToCombine = new HashSet<VecI>();
        List<IChangeInfo> changes = new();

        var ordererd = OrderLayers(layersToCombine, target);

        if (ordererd.Count == 0)
        {
            return changes;
        }

        foreach (var guid in ordererd)
        {
            var layer = target.FindMemberOrThrow<StructureNode>(guid);

            AddMissingKeyFrame(targetLayer, frame, layer, changes, target);

            if (layer is not IRasterizable or ImageLayerNode)
                continue;

            if (layer is ImageLayerNode imageLayerNode)
            {
                var layerImage = imageLayerNode.GetLayerImageAtFrame(frame);
                if (layerImage is null)
                    continue;

                chunksToCombine.UnionWith(layerImage.FindAllChunks());
            }
            else
            {
                AddChunksByTightBounds(layer, chunksToCombine, frame);
            }
        }

        bool allVector = layersToCombine.All(x => target.FindMember(x) is VectorLayerNode);

        AffectedArea affArea = new();

        // TODO: add custom layer merge
        if (!allVector)
        {
            affArea = RasterMerge(target, targetLayer, frame);
        }
        else
        {
            affArea = VectorMerge(target, targetLayer, frame, layersToCombine);
        }

        changes.Add(new LayerImageArea_ChangeInfo(targetLayerGuid, affArea));
        return changes;
    }

    private AffectedArea VectorMerge(Document target, StructureNode targetLayer, int frame, HashSet<Guid> toCombine)
    {
        if (targetLayer is not VectorLayerNode vectorLayer)
            throw new InvalidOperationException("Target layer is not a vector layer");

        ShapeVectorData targetData = vectorLayer.EmbeddedShapeData ?? null;
        VectorPath? targetPath = targetData?.ToPath();

        var reversed = toCombine.Reverse().ToHashSet();

        foreach (var guid in reversed)
        {
            if (target.FindMember(guid) is not VectorLayerNode vectorNode)
                continue;

            if (vectorNode.EmbeddedShapeData == null)
                continue;

            VectorPath path = vectorNode.EmbeddedShapeData.ToPath();

            if (targetData == null)
            {
                targetData = vectorNode.EmbeddedShapeData;
                targetPath = new VectorPath();
                targetPath.AddPath(path, vectorNode.EmbeddedShapeData.TransformationMatrix, AddPathMode.Append);

                if (originalPaths.ContainsKey(frame))
                    originalPaths[frame].Dispose();

                originalPaths[frame] = new VectorPath(path);
            }
            else
            {
                if (targetPath == null)
                {
                    targetPath = new VectorPath();
                }

                targetPath.AddPath(path, vectorNode.EmbeddedShapeData.TransformationMatrix, AddPathMode.Append);
                path.Dispose();
            }
        }

        var clone = targetData.Clone();
        PathVectorData data;
        if (clone is not PathVectorData vectorData)
        {
            ShapeVectorData shape = clone as ShapeVectorData;
            data = new PathVectorData(targetPath)
            {
                Stroke = shape?.Stroke,
                FillPaintable = shape?.FillPaintable,
                StrokeWidth = shape?.StrokeWidth ?? 1,
                Fill = shape?.Fill ?? true,
                TransformationMatrix = Matrix3X3.Identity
            };
        }
        else
        {
            data = vectorData;
            data.TransformationMatrix = Matrix3X3.Identity;
            data.Path = targetPath;
        }

        vectorLayer.EmbeddedShapeData = data;

        return new AffectedArea(new HashSet<VecI>());
    }

    private AffectedArea RasterMerge(Document target, StructureNode targetLayer, int frame)
    {
        if (targetLayer is not ImageLayerNode)
            throw new InvalidOperationException("Target layer is not a raster layer");

        var toDrawOnImage = ((ImageLayerNode)targetLayer).GetLayerImageAtFrame(frame);
        toDrawOnImage.EnqueueClear();

        Texture tempTexture = Texture.ForProcessing(target.Size, target.ProcessingColorSpace);

        DocumentRenderer renderer = new(target);

        AffectedArea affArea = new();
        DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
        {
            renderer.RenderLayers(tempTexture.DrawingSurface, layersToCombine, frame, ChunkResolution.Full,
                target.Size);

            toDrawOnImage.EnqueueDrawTexture(VecI.Zero, tempTexture);

            affArea = toDrawOnImage.FindAffectedArea();
            originalChunks.Add(frame, new CommittedChunkStorage(toDrawOnImage, affArea.Chunks));
            toDrawOnImage.CommitChanges();

            tempTexture.Dispose();
        });
        return affArea;
    }

    private HashSet<Guid> OrderLayers(HashSet<Guid> layersToCombine, Document document)
    {
        HashSet<Guid> ordered = new();
        document.NodeGraph.TryTraverse(node =>
        {
            if (node is LayerNode layer && layersToCombine.Contains(layer.Id))
            {
                ordered.Add(layer.Id);
            }
        });

        return ordered.Reverse().ToHashSet();
    }

    private void AddMissingKeyFrame(StructureNode targetLayer, int frame, StructureNode layer,
        List<IChangeInfo> changes,
        Document target)
    {
        bool hasKeyframe = targetLayer.KeyFrames.Any(x => x.IsInFrame(frame));
        if (hasKeyframe)
            return;

        if (layer is not ImageLayerNode)
            return;

        var keyFrameData = layer.KeyFrames.FirstOrDefault(x => x.IsInFrame(frame));
        if (keyFrameData is null)
            return;

        var clonedData = keyFrameData.Clone(true);

        targetLayer.AddFrame(keyFrameData.KeyFrameGuid, clonedData);

        changes.Add(new CreateRasterKeyFrame_ChangeInfo(targetLayerGuid, frame, clonedData.KeyFrameGuid, true));
        changes.Add(new KeyFrameLength_ChangeInfo(targetLayerGuid, clonedData.StartFrame, clonedData.Duration));

        target.AnimationData.AddKeyFrame(new RasterKeyFrame(clonedData.KeyFrameGuid, targetLayerGuid, frame, target));
    }

    private int GetMaxFrame(Document target, StructureNode targetLayer)
    {
        if (targetLayer.KeyFrames.Count == 0)
            return 0;

        int maxFrame = targetLayer.KeyFrames.Max(x => x.StartFrame + x.Duration);
        foreach (var toMerge in membersToMerge)
        {
            var member = target.FindMemberOrThrow<StructureNode>(toMerge);
            if (member.KeyFrames.Count > 0)
            {
                maxFrame = Math.Max(maxFrame, member.KeyFrames.Max(x => x.StartFrame + x.Duration));
            }
        }

        return maxFrame;
    }

    private void AddChunksByTightBounds(StructureNode layer, HashSet<VecI> chunksToCombine, int frame)
    {
        var tightBounds = layer.GetTightBounds(frame);
        if (tightBounds.HasValue)
        {
            VecI chunk = (VecI)tightBounds.Value.TopLeft / ChunkyImage.FullChunkSize;
            VecI sizeInChunks = ((VecI)tightBounds.Value.Size / ChunkyImage.FullChunkSize);
            sizeInChunks = new VecI(Math.Max(1, sizeInChunks.X), Math.Max(1, sizeInChunks.Y));
            for (int x = 0; x < sizeInChunks.X; x++)
            {
                for (int y = 0; y < sizeInChunks.Y; y++)
                {
                    chunksToCombine.Add(chunk + new VecI(x, y));
                }
            }
        }
    }

    private IChangeInfo RevertFrame(LayerNode targetLayer, int frame)
    {
        if (targetLayer is ImageLayerNode imageLayerNode)
        {
            return RasterRevert(imageLayerNode, frame);
        }
        else if (targetLayer is VectorLayerNode vectorLayerNode)
        {
            return VectorRevert(vectorLayerNode, frame);
        }

        throw new InvalidOperationException("Layer type not supported");
    }

    private IChangeInfo RasterRevert(ImageLayerNode targetLayer, int frame)
    {
        var toDrawOnImage = targetLayer.GetLayerImageAtFrame(frame);
        if (toDrawOnImage is null)
            throw new InvalidOperationException("Layer image not found");

        toDrawOnImage.EnqueueClear();

        CommittedChunkStorage? storedChunks = originalChunks[frame];

        var affectedArea =
            DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(
                targetLayer.GetLayerImageAtFrame(frame),
                ref storedChunks);

        toDrawOnImage.CommitChanges();
        return new LayerImageArea_ChangeInfo(targetLayerGuid, affectedArea);
    }

    private IChangeInfo VectorRevert(VectorLayerNode targetLayer, int frame)
    {
        if (!originalPaths.TryGetValue(frame, out var path))
            throw new InvalidOperationException("Original path not found");

        targetLayer.EmbeddedShapeData = new PathVectorData(path);
        return new VectorShape_ChangeInfo(targetLayer.Id, new AffectedArea(new HashSet<VecI>()));
    }

    public override void Dispose()
    {
        foreach (var originalChunk in originalChunks)
        {
            originalChunk.Value.Dispose();
        }

        originalChunks.Clear();

        foreach (var originalPath in originalPaths)
        {
            originalPath.Value.Dispose();
        }

        originalPaths.Clear();
    }
}
