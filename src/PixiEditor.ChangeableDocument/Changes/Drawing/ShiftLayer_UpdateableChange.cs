using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ShiftLayer_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private bool keepOriginal;
    private VecI delta;
    private CommittedChunkStorage? originalLayerChunks;

    [GenerateUpdateableChangeActions]
    public ShiftLayer_UpdateableChange(Guid layerGuid, VecI delta, bool keepOriginal)
    {
        this.delta = delta;
        this.layerGuid = layerGuid;
        this.keepOriginal = keepOriginal;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.HasMember<Layer>(layerGuid);
    }

    [UpdateChangeMethod]
    public void Update(VecI delta, bool keepOriginal)
    {
        this.delta = delta;
        this.keepOriginal = keepOriginal;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, keepOriginal, delta);
        var image = target.FindMemberOrThrow<Layer>(layerGuid).LayerImage;

        if (originalLayerChunks is not null)
            throw new InvalidOperationException("saved chunks is not null even though we are trying to save them again");
        originalLayerChunks = new(image, image.FindAffectedChunks());
        image.CommitChanges();

        ignoreInUndo = delta.TaxicabLength == 0;
        return new LayerImageChunks_ChangeInfo(layerGuid, chunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var chunks = ShiftLayerHelper.DrawShiftedLayer(target, layerGuid, keepOriginal, delta);
        return new LayerImageChunks_ChangeInfo(layerGuid, chunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var image = target.FindMemberOrThrow<Layer>(layerGuid).LayerImage;
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalLayerChunks);
        return new LayerImageChunks_ChangeInfo(layerGuid, affected);
    }

    public override void Dispose()
    {
        originalLayerChunks?.Dispose();
    }
}
