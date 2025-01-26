using System.Runtime.InteropServices;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ShiftLayer_UpdateableChange : Change
{
    private List<Guid> layerGuids;
    private VecD delta;
    private Dictionary<Guid, CommittedChunkStorage?> originalLayerChunks = new();
    private Dictionary<Guid, Matrix3X3> originalTransformations = new();

    private int frame;

    [GenerateMakeChangeAction]
    public ShiftLayer_UpdateableChange(List<Guid> layerGuids, VecD delta, int frame)
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

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        originalLayerChunks = new Dictionary<Guid, CommittedChunkStorage?>();
        originalTransformations = new Dictionary<Guid, Matrix3X3>();
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is ImageLayerNode)
            {
                var area = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, false, (VecI)delta, frame);
                var image = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);

                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, area));

                originalLayerChunks[layerGuid] = new(image, image.FindAffectedArea().Chunks);
                image.CommitChanges();
            }
            else if (layer is ITransformableObject transformableObject)
            {
                originalTransformations[layerGuid] = transformableObject.TransformationMatrix;
                AffectedArea affected = AffectedAreaFromBounds(target, layerGuid, frame);
                transformableObject.TransformationMatrix = transformableObject.TransformationMatrix.PostConcat(
                Matrix3X3.CreateTranslation((float)delta.X, (float)delta.Y));
                changes.Add(new VectorShape_ChangeInfo(layerGuid, affected));
            }
        }

        ignoreInUndo = delta.TaxicabLength == 0;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is ImageLayerNode)
            {
                var image = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);
                CommittedChunkStorage? originalChunks = originalLayerChunks[layerGuid];
                var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
                changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
            }
            else if (layer is ITransformableObject transformableObject)
            {
                transformableObject.TransformationMatrix = originalTransformations[layerGuid];
            }
        }

        return changes;
    }

    public override void Dispose()
    {
        foreach (var (_, value) in originalLayerChunks)
        {
            value?.Dispose();
        }
    }
    
    internal static AffectedArea AffectedAreaFromBounds(Document target, Guid layerGuid, int frame)
    {
        HashSet<VecI> chunks = new HashSet<VecI>();
        
        var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);
        if (layer is not VectorLayerNode vectorLayer)
        {
            return new AffectedArea();
        }

        RectD? bounds = vectorLayer.GetTightBounds(frame);
        if (bounds is null)
        {
            return new AffectedArea();
        }
        
        RectD boundsValue = bounds.Value;

        int chunkSize = ChunkyImage.FullChunkSize;
        
        VecI start = new VecI((int)boundsValue.X / chunkSize, (int)boundsValue.Y / chunkSize);
        VecI end = new VecI((int)(boundsValue.X + boundsValue.Width) / chunkSize, (int)(boundsValue.Y + boundsValue.Height) / chunkSize);
        
        HashSet<VecI> affectedChunks = new HashSet<VecI>();
        
        for (int x = start.X; x <= end.X; x++)
        {
            for (int y = start.Y; y <= end.Y; y++)
            {
                affectedChunks.Add(new VecI(x, y));
            }
        }
        
        return new AffectedArea(affectedChunks);
    }
}
