namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ShiftLayer_UpdateableChange : UpdateableChange
{
    private readonly Guid layerGuid;
    private VecI delta;
    private CommittedChunkStorage? originalLayerChunks;

    [GenerateUpdateableChangeActions]
    public ShiftLayer_UpdateableChange(Guid layerGuid, VecI delta)
    {
        this.delta = delta;
        this.layerGuid = layerGuid;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        var member = target.FindMember(layerGuid);
        if (member is not Layer)
            return new Error();
        return new Success();
    }

    [UpdateChangeMethod]
    public void Update(VecI delta)
    {
        this.delta = delta;
    }

    private HashSet<VecI> DrawShiftedLayer(Document target)
    {
        var targetImage = ((Layer)target.FindMemberOrThrow(layerGuid)).LayerImage;
        var prevChunks = targetImage.FindAffectedChunks();
        targetImage.CancelChanges();
        targetImage.EnqueueClear();
        targetImage.EnqueueDrawChunkyImage(delta, targetImage, false, false);
        var curChunks = targetImage.FindAffectedChunks();
        curChunks.UnionWith(prevChunks);
        return curChunks;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, out bool ignoreInUndo)
    {
        var chunks = DrawShiftedLayer(target);
        var image = ((Layer)target.FindMemberOrThrow(layerGuid)).LayerImage;

        if (originalLayerChunks is not null)
            throw new InvalidOperationException("saved chunks is not null even though we are trying to save them again");
        originalLayerChunks = new(image, image.FindAffectedChunks());
        image.CommitChanges();

        ignoreInUndo = delta.TaxicabLength == 0;
        return new LayerImageChunks_ChangeInfo(layerGuid, chunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        var chunks = DrawShiftedLayer(target);
        return new LayerImageChunks_ChangeInfo(layerGuid, chunks);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var image = ((Layer)target.FindMemberOrThrow(layerGuid)).LayerImage;
        var affected = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(image, ref originalLayerChunks);
        return new LayerImageChunks_ChangeInfo(layerGuid, affected);
    }

    public override void Dispose()
    {
        originalLayerChunks?.Dispose();
    }
}
