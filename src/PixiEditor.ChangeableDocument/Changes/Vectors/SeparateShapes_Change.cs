using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class SeparateShapes_Change : Change
{
    private readonly Guid memberId;
    private PathVectorData originalData;
    private List<Guid> newMemberIds = new List<Guid>();
    private Dictionary<Guid, VecD> originalPositions = new Dictionary<Guid, VecD>();

    [GenerateMakeChangeAction]
    public SeparateShapes_Change(Guid memberId)
    {
        this.memberId = memberId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindMember<VectorLayerNode>(memberId, out VectorLayerNode? node))
        {
            // Check if the node has embedded shape data and is not already a PathVectorData
            return node.EmbeddedShapeData is PathVectorData p && GetShapeCount(p) > 1;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        PathVectorData data = node.EmbeddedShapeData as PathVectorData ??
                              throw new InvalidOperationException("Node does not contain PathVectorData.");
        originalData = data.Clone() as PathVectorData;

        // Separate the shapes into individual PathVectorData instances
        List<PathVectorData> separatedShapes = new List<PathVectorData>();
        var editablePath = new EditableVectorPath(data.Path);
        foreach (var subShape in editablePath.SubShapes)
        {
            PathVectorData newShape = new PathVectorData(subShape.ToPath())
            {
                Fill = data.Fill,
                FillPaintable = data.FillPaintable,
                Stroke = data.Stroke,
                StrokeWidth = data.StrokeWidth,
                TransformationMatrix = data.TransformationMatrix,
                FillType = data.FillType,
                StrokeLineCap = data.StrokeLineCap,
                StrokeLineJoin = data.StrokeLineJoin,
            };

            separatedShapes.Add(newShape);
        }

        // Replace the original data with the first separated shape
        node.EmbeddedShapeData = separatedShapes[0];
        ignoreInUndo = false;

        List<IChangeInfo> changes = new List<IChangeInfo>();
        changes.Add(new VectorShape_ChangeInfo(
            memberId,
            new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
                (RectI)node.EmbeddedShapeData.TransformedVisualAABB, ChunkyImage.FullChunkSize))));

        var previousNode = node;

        for (int i = 1; i < separatedShapes.Count; i++)
        {
            // Create a new node for each separated shape
            VectorLayerNode newNode = node.Clone(false) as VectorLayerNode;

            if (firstApply)
            {
                newMemberIds.Add(newNode.Id);
            }
            else
            {
                newNode.Id = newMemberIds[i - 1];
            }

            newNode.EmbeddedShapeData = separatedShapes[i];

            newNode.MemberName = $"{node.MemberName} (Shape {i + 1})"; // Rename to indicate it's a separate shape

            target.NodeGraph.AddNode(newNode);
            changes.Add(CreateLayer_ChangeInfo.FromLayer(newNode));
            var appended = NodeOperations.AppendMember(previousNode, newNode, out var positions);
            AppendPositions(positions);
            changes.AddRange(appended);

            previousNode = newNode;
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        node.EmbeddedShapeData = originalData.Clone() as PathVectorData;

        List<IChangeInfo> changes = new List<IChangeInfo>();

        var aabb = node.EmbeddedShapeData.TransformedVisualAABB;
        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        changes.Add(new VectorShape_ChangeInfo(memberId, affected));

        changes.AddRange(NodeOperations.RevertPositions(originalPositions, target));

        // Remove the newly created nodes
        foreach (var newMemberId in newMemberIds)
        {
            var createdNode = target.FindNode<VectorLayerNode>(newMemberId);
            if (createdNode != null)
            {
                changes.AddRange(NodeOperations.DetachStructureNode(createdNode));
                changes.Add(new DeleteStructureMember_ChangeInfo(newMemberId));

                target.NodeGraph.RemoveNode(createdNode);
                createdNode?.Dispose();
            }
        }

        originalPositions.Clear();

        return changes;
    }

    private int GetShapeCount(PathVectorData data)
    {
        return new EditableVectorPath(data.Path).SubShapes.Count;
    }

    private void AppendPositions(Dictionary<Guid, VecD> positions)
    {
        foreach (var position in positions)
        {
            originalPositions[position.Key] = position.Value;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        originalData?.Path?.Dispose();
    }
}
