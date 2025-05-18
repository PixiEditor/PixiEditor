using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class PreviewShiftLayers_UpdateableChange : InterruptableUpdateableChange
{
    private List<Guid> layerGuids;
    private VecD delta;
    private Dictionary<Guid, ShapeVectorData> originalShapes;

    private int frame;

    [GenerateUpdateableChangeActions]
    public PreviewShiftLayers_UpdateableChange(List<Guid> layerGuids, VecD delta, int frame)
    {
        this.delta = delta;
        this.layerGuids = layerGuids;
        this.frame = frame;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (layerGuids.Count == 0)
        {
            return false;
        }

        layerGuids = target.ExtractLayers(layerGuids);

        foreach (var layer in layerGuids)
        {
            if (!target.HasMember(layer)) return false;
        }

        originalShapes = new Dictionary<Guid, ShapeVectorData>();

        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is VectorLayerNode transformableObject)
            {
                originalShapes[layerGuid] = transformableObject.EmbeddedShapeData;
                transformableObject.EmbeddedShapeData = null;
            }
        }

        return true;
    }

    [UpdateChangeMethod]
    public void Update(VecD delta)
    {
        this.delta = delta;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is ImageLayerNode)
            {
                var area = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, true, (VecI)delta, frame);
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, area));
            }
            else if (layer is VectorLayerNode vectorLayer)
            {
                StrokeJoin join = StrokeJoin.Miter;
                StrokeCap cap = StrokeCap.Butt;
                
                (vectorLayer.EmbeddedShapeData as PathVectorData)?.Path.Dispose();

                var originalShape = originalShapes[layerGuid];

                var path = originalShape.ToPath();

                if (originalShape is PathVectorData shape)
                {
                    join = shape.StrokeLineJoin;
                    cap = shape.StrokeLineCap;
                }

                VecD mappedDelta = originalShape.TransformationMatrix.Invert().MapVector((float)delta.X, (float)delta.Y);
                
                var finalMatrix = Matrix3X3.CreateTranslation((float)mappedDelta.X, (float)mappedDelta.Y);

                path.AddPath(path, finalMatrix, AddPathMode.Append);

                var newShapeData = new PathVectorData(path)
                {
                    StrokeWidth = originalShape.StrokeWidth,
                    Stroke = originalShape.Stroke,
                    FillPaintable = originalShape.FillPaintable,
                    Fill = originalShape.Fill,
                    TransformationMatrix = originalShape.TransformationMatrix,
                    StrokeLineJoin = join,
                    StrokeLineCap = cap
                };
                
                vectorLayer.EmbeddedShapeData = newShapeData;
                changes.Add(new VectorShape_ChangeInfo(layerGuid, ShiftLayer_UpdateableChange.AffectedAreaFromBounds(target, layerGuid, frame)));
            }
        }

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        ignoreInUndo = true;
        return RevertPreview(target);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        return RevertPreview(target);
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> RevertPreview(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is ImageLayerNode imgLayer)
            {
                var image = imgLayer.GetLayerImageAtFrame(frame);
                var affected = image.FindAffectedArea();
                image.CancelChanges();
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
            }
            else if (layer is VectorLayerNode transformableObject)
            {
                (transformableObject.EmbeddedShapeData as PathVectorData)?.Path.Dispose();
                transformableObject.EmbeddedShapeData = originalShapes[layerGuid];
            }
        }

        return changes;
    }
}
