using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class CenterContent_Change : Change
{
    private VecI _oldOffset;
    private List<Guid> affectedLayers;
    private Dictionary<Guid, CommittedChunkStorage>? originalLayerChunks;

    [GenerateMakeChangeAction]
    public CenterContent_Change(List<Guid> layers)
    {
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
            Layer layer = document.FindMemberOrThrow<Layer>(layerGuid);
            RectI? tightBounds = layer.GetTightBounds();
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

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        VecI documentCenter = target.Size / 2;
        VecI currentOffset = _oldOffset;
        
        VecI shift = documentCenter - currentOffset;

        List<IChangeInfo> changes = new List<IChangeInfo>();
        originalLayerChunks = new Dictionary<Guid, CommittedChunkStorage>();
        
        foreach (var layerGuid in affectedLayers)
        {
            RasterLayer layer = target.FindMemberOrThrow<RasterLayer>(layerGuid);
            var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, false, shift);
            changes.Add(new LayerImageArea_ChangeInfo(layerGuid, chunks));

            // TODO: Adding support for non-raster layer should be easy, add
            
            originalLayerChunks[layerGuid] = new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedArea().Chunks);
            layer.LayerImage.CommitChanges();
        }

        ignoreInUndo = shift.TaxicabLength == 0;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in affectedLayers)
        {
            var image = target.FindMemberOrThrow<RasterLayer>(layerGuid).LayerImage;
            CommittedChunkStorage? originalChunks = originalLayerChunks?[layerGuid];
            var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
            changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
        }
        
        return changes;
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
