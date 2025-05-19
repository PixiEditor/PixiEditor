using ChunkyImageLib.Operations;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Vectors;

internal class ConvertToCurve_Change : Change
{
    public readonly Guid memberId;

    private ShapeVectorData originalData;
    private bool originalHighDpiRendering;

    [GenerateMakeChangeAction]
    public ConvertToCurve_Change(Guid memberId)
    {
        this.memberId = memberId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (target.TryFindNode(memberId, out VectorLayerNode? node))
        {
            return node.EmbeddedShapeData != null && node.EmbeddedShapeData is not PathVectorData;
        }

        return false;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        originalData = node.EmbeddedShapeData;

        // TODO: Stroke Line cap and join is missing? Validate
        node.EmbeddedShapeData = new PathVectorData(originalData.ToPath())
        {
            Fill = originalData.Fill,
            FillPaintable = originalData.FillPaintable,
            Stroke = originalData.Stroke,
            StrokeWidth = originalData.StrokeWidth,
            TransformationMatrix = originalData.TransformationMatrix
        };

        originalHighDpiRendering = node.AllowHighDpiRendering;
        node.AllowHighDpiRendering = true;

        ignoreInUndo = false;

        var aabb = node.EmbeddedShapeData.TransformedVisualAABB;
        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(memberId, affected);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        VectorLayerNode node = target.FindNodeOrThrow<VectorLayerNode>(memberId);
        node.EmbeddedShapeData = originalData;

        node.AllowHighDpiRendering = originalHighDpiRendering;

        var aabb = node.EmbeddedShapeData.TransformedVisualAABB;
        var affected = new AffectedArea(OperationHelper.FindChunksTouchingRectangle(
            (RectI)aabb, ChunkyImage.FullChunkSize));

        return new VectorShape_ChangeInfo(memberId, affected);
    }
}
