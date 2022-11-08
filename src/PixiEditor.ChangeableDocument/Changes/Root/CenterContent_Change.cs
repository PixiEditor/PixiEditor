using PixiEditor.ChangeableDocument.Changes.Drawing;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class CenterContent_Change : Change
{
    private VecI _oldOffset;
    private List<Guid> _affectedLayers;
    private Dictionary<Guid, CommittedChunkStorage>? originalLayerChunks;

    [GenerateMakeChangeAction]
    public CenterContent_Change(List<Guid> layers)
    {
        _affectedLayers = layers;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (_affectedLayers.Count == 0)
        {
            return false;
        }

        _affectedLayers = target.ExtractLayers(_affectedLayers);

        foreach (var layer in _affectedLayers)
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
        foreach (var layerGuid in _affectedLayers)
        {
            Layer layer = document.FindMemberOrThrow<Layer>(layerGuid);
            RectI? tightBounds = layer.LayerImage.FindPreciseBounds();
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
        
        foreach (var layerGuid in _affectedLayers)
        {
            Layer layer = target.FindMemberOrThrow<Layer>(layerGuid);
            var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, false, shift);
            changes.Add(new LayerImageChunks_ChangeInfo(layerGuid, chunks));
            
            originalLayerChunks[layerGuid] = new CommittedChunkStorage(layer.LayerImage, layer.LayerImage.FindAffectedChunks());
            layer.LayerImage.CommitChanges();
        }

        ignoreInUndo = shift.TaxicabLength == 0;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in _affectedLayers)
        {
            var image = target.FindMemberOrThrow<Layer>(layerGuid).LayerImage;
            CommittedChunkStorage? originalChunks = originalLayerChunks?[layerGuid];
            var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
            changes.Add(new LayerImageChunks_ChangeInfo(layerGuid, affected));
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
