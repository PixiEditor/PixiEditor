using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class SetShapeGeometry_UpdateableChange : UpdateableChange
{
    public Guid TargetId { get; set; }
    public ShapeVectorData Data { get; set; }

    private ShapeVectorData? originalData;

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

        return new VectorShape_ChangeInfo(node.Id);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = false;
        var node = target.FindNode<VectorLayerNode>(TargetId);
        node.ShapeData = Data;

        return new VectorShape_ChangeInfo(node.Id);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var node = target.FindNode<VectorLayerNode>(TargetId);
        node.ShapeData = originalData;

        return new VectorShape_ChangeInfo(node.Id);
    }
}
