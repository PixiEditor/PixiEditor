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
    public VectorShapeChangeType ChangeType { get; set; }

    private ShapeVectorData? originalData;

    private AffectedArea lastAffectedArea;

    [GenerateUpdateableChangeActions]
    public SetShapeGeometry_UpdateableChange(Guid targetId, ShapeVectorData data, VectorShapeChangeType changeType)
    {
        TargetId = targetId;
        Data = data;
        ChangeType = changeType;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode<VectorLayerNode>(TargetId, out var node))
        {
            if (IsIdentical(node.EmbeddedShapeData, Data))
            {
                return false;
            }

            originalData = (ShapeVectorData?)node.EmbeddedShapeData?.Clone();
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

        node.EmbeddedShapeData = Data;

        RectD aabb = node.EmbeddedShapeData.TransformedAABB.RoundOutwards();
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
        if (node == null)
        {
            return new None();
        }

        node.EmbeddedShapeData = Data;

        RectD aabb = node.EmbeddedShapeData.TransformedAABB.RoundOutwards();
        aabb = aabb with { Size = new VecD(Math.Max(1, aabb.Size.X), Math.Max(1, aabb.Size.Y)) };

        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(node.Id, affected);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode<VectorLayerNode>(TargetId);
        node.EmbeddedShapeData = originalData;

        AffectedArea affected = new AffectedArea();

        if (node.EmbeddedShapeData != null)
        {
            RectD aabb = node.EmbeddedShapeData.TransformedAABB.RoundOutwards();
            aabb = aabb with { Size = new VecD(Math.Max(1, aabb.Size.X), Math.Max(1, aabb.Size.Y)) };

            affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                (RectI)aabb, ChunkyImage.FullChunkSize));
        }

        return new VectorShape_ChangeInfo(node.Id, affected);
    }

    private bool IsIdentical(ShapeVectorData? a, ShapeVectorData? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.GetType() != b.GetType())
        {
            return false;
        }

        return a.Equals(b);
    }

    public override bool IsMergeableWith(Change other)
    {
        if (other is SetShapeGeometry_UpdateableChange change)
        {
            return change.TargetId == TargetId &&
                   !ChangeType.HasFlag(VectorShapeChangeType.GeometryData) && ChangeType.HasFlag(change.ChangeType) &&
                   change.Data is not TextVectorData; // text should not be merged into one change
        }

        return false;
    }
}

[Flags]
public enum VectorShapeChangeType
{
    GeometryData = 1,
    Stroke = 2,
    Fill = 4,
    TransformationMatrix = 8,
    OtherVisuals = 16,
    All = ~0
}
