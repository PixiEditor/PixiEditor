using System.Runtime.InteropServices;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;

internal class ShiftLayer_UpdateableChange : UpdateableChange
{
    private List<Guid> layerGuids;
    private bool keepOriginal;
    private VecI delta;
    private Dictionary<Guid, CommittedChunkStorage?> originalLayerChunks = new();

    private List<IChangeInfo> _tempChanges = new();
    private int frame;

    [GenerateUpdateableChangeActions]
    public ShiftLayer_UpdateableChange(List<Guid> layerGuids, VecI delta, bool keepOriginal, int frame)
    {
        this.delta = delta;
        this.layerGuids = layerGuids;
        this.keepOriginal = keepOriginal;
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

    [UpdateChangeMethod]
    public void Update(VecI delta, bool keepOriginal)
    {
        this.delta = delta;
        this.keepOriginal = keepOriginal;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        originalLayerChunks = new Dictionary<Guid, CommittedChunkStorage?>();
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            // TODO: This now does't crash, but ignores other layer types. Think how to handle this.
            if (layer is not ImageLayerNode)
            {
                continue;
            }

            var area = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, keepOriginal, delta, frame);
            var image = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);

            changes.Add(new LayerImageArea_ChangeInfo(layerGuid, area));

            originalLayerChunks[layerGuid] = new(image, image.FindAffectedArea().Chunks);
            image.CommitChanges();
        }

        ignoreInUndo = delta.TaxicabLength == 0;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        _tempChanges.Clear();

        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is not ImageLayerNode)
            {
                continue;
            }

            var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, keepOriginal, delta, frame);
            _tempChanges.Add(new LayerImageArea_ChangeInfo(layerGuid, chunks));
        }

        return _tempChanges;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new List<IChangeInfo>();
        foreach (var layerGuid in layerGuids)
        {
            var layer = target.FindMemberOrThrow<LayerNode>(layerGuid);

            if (layer is not ImageLayerNode)
            {
                continue;
            }

            var image = target.FindMemberOrThrow<ImageLayerNode>(layerGuid).GetLayerImageAtFrame(frame);
            CommittedChunkStorage? originalChunks = originalLayerChunks[layerGuid];
            var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalChunks);
            changes.Add(new LayerImageArea_ChangeInfo(layerGuid, affected));
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
}
