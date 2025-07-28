using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.Drawing;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Objects;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class CenterContent_Change : Change
{
    private VecI _oldOffset;
    private List<Guid> affectedLayers;
    private Dictionary<Guid, CommittedChunkStorage>? originalLayerChunks;
    private int frame;
    private VecD oldDelta;

    [GenerateMakeChangeAction]
    public CenterContent_Change(List<Guid> layers, int frame)
    {
        this.frame = frame;
        affectedLayers = layers;
    }

    public override bool InitializeAndValidate(Document target)
    {
        if (affectedLayers.Count == 0)
        {
            return false;
        }

        affectedLayers = target.ExtractLayers(affectedLayers);

        foreach (var layer in affectedLayers)
        {
            if (!target.HasMember(layer)) return false;
        }

        _oldOffset = CalculateCurrentOffset(target);

        return true;
    }

    private VecI CalculateCurrentOffset(Document document)
    {
        VecI currentCenter = new VecI(0, 0);
        RectI? currentBounds = null;
        foreach (var layerGuid in affectedLayers)
        {
            LayerNode layer = document.FindMemberOrThrow<LayerNode>(layerGuid);
            RectI? tightBounds = (RectI)layer.GetTightBounds(frame);
            if (tightBounds.HasValue)
            {
                currentBounds = currentBounds.HasValue ? currentBounds.Value.Union(tightBounds.Value) : tightBounds;
                currentCenter = new VecI(
                    currentBounds.Value.X + currentBounds.Value.Width / 2,
                    currentBounds.Value.Y + currentBounds.Value.Height / 2);
            }
        }

        return currentCenter;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        VecI documentCenter = target.Size / 2;
        VecI currentOffset = _oldOffset;
        
        VecI shift = documentCenter - currentOffset;

        oldDelta = shift;
        
        List<IChangeInfo> changes = new List<IChangeInfo>();
        originalLayerChunks = new Dictionary<Guid, CommittedChunkStorage>();

        foreach (var layerGuid in affectedLayers)
        {
            LayerNode node = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (node is ImageLayerNode imageLayerNode)
            {
                var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, false, shift, frame);
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, chunks));
                var image = imageLayerNode.GetLayerImageAtFrame(frame);
                originalLayerChunks[layerGuid] = new CommittedChunkStorage(image, image.FindAffectedArea().Chunks);
                image.CommitChanges();
            }
            else if (node is ITransformableObject transformable)
            {
                transformable.TransformationMatrix = transformable.TransformationMatrix.PostConcat(
                    Matrix3X3.CreateTranslation(shift.X, shift.Y));
                var affectedArea = FromSize(target);
                changes.Add(new TransformObject_ChangeInfo(layerGuid, affectedArea));
            }
        }

        ignoreInUndo = shift.TaxicabLength == 0;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in affectedLayers)
        {
            var layerNode = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layerNode is ImageLayerNode imageNode)
            {
                var image = imageNode.GetLayerImageAtFrame(frame);
                CommittedChunkStorage? originalChunks = originalLayerChunks?[layerGuid];
                var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
            }
            else if (layerNode is ITransformableObject transformable)
            {
                transformable.TransformationMatrix = transformable.TransformationMatrix.PostConcat(
                    Matrix3X3.CreateTranslation((float)-oldDelta.X, (float)-oldDelta.Y));
                
                var affectedArea = FromSize(target);
                changes.Add(new TransformObject_ChangeInfo(layerGuid, affectedArea));
            }
        }

        return changes;
    }

    private AffectedArea FromSize(Document target)
    {
        RectI bounds = new RectI(VecI.Zero, target.Size);
        return new AffectedArea(OperationHelper.FindChunksTouchingRectangle(bounds, ChunkyImage.FullChunkSize));
    }

    public override void Dispose()
    {
        if (originalLayerChunks == null)
        {
            return;
        }

        foreach (var layerChunk in originalLayerChunks)
        {
            layerChunk.Value.Dispose();
        }

        originalLayerChunks = null;
    }
}
