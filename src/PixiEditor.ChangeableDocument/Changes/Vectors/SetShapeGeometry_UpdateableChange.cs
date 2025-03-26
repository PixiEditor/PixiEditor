using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class SetShapeGeometry_UpdateableChange : InterruptableUpdateableChange
{
    public Guid TargetId { get; set; }
    public ShapeVectorData Data { get; set; }

    private ShapeVectorData? originalData;

    private AffectedArea lastAffectedArea;

    [GenerateUpdateableChangeActions]
    public SetShapeGeometry_UpdateableChange(Guid targetId, ShapeVectorData data)
    {
        TargetId = targetId;
        Data = data;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode<VectorLayerNode>(TargetId, out var node))
        {
            // TODO: Add is identical check
            originalData = (ShapeVectorData?)node.ShapeData?.Clone();
            return true;
        }

        return false;
    }

    [UpdateChangeMethod]
    public void Update(ShapeVectorData data)
    {
        Data = data;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var node = target.FindNode<VectorLayerNode>(TargetId);

        node.ShapeData = Data;
        
        RectD aabb = node.ShapeData.TransformedAABB.RoundOutwards();
        aabb = aabb with { Size = new VecD(Math.Max(1, aabb.Size.X), Math.Max(1, aabb.Size.Y)) };

        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        var tmp = new AffectedArea(affected);

        if (lastAffectedArea.Chunks != null)
        {
            affected.UnionWith(lastAffectedArea);
        }

        lastAffectedArea = tmp;

        return new VectorShape_ChangeInfo(node.Id, affected);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        var node = target.FindNode<VectorLayerNode>(TargetId);
        node.ShapeData = Data;

        RectD aabb = node.ShapeData.TransformedAABB.RoundOutwards();
        aabb = aabb with { Size = new VecD(Math.Max(1, aabb.Size.X), Math.Max(1, aabb.Size.Y)) };

        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(node.Id, affected);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode<VectorLayerNode>(TargetId);
        node.ShapeData = originalData;

        AffectedArea affected = new AffectedArea();

        if (node.ShapeData != null)
        {
            RectD aabb = node.ShapeData.TransformedAABB.RoundOutwards();
            aabb = aabb with { Size = new VecD(Math.Max(1, aabb.Size.X), Math.Max(1, aabb.Size.Y)) };

            affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                (RectI)aabb, ChunkyImage.FullChunkSize));
        }

        return new VectorShape_ChangeInfo(node.Id, affected);
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is SetShapeGeometry_UpdateableChange change)
        {
            return change.TargetId == TargetId && change.Data is not TextVectorData; // text should not be merged into one change
        }

        return false;
    }
}
