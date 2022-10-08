using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ReplaceColor_Change : Change
{
    private readonly Color oldColor;
    private readonly Color newColor;

    private Dictionary<Guid, CommittedChunkStorage>? savedChunks;

    [GenerateMakeChangeAction]
    public ReplaceColor_Change(Color oldColor, Color newColor)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
    }

    public override OneOf<Success, Error> InitializeAndValidate(Document target)
    {
        return new Success();
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (savedChunks is not null)
            throw new InvalidOperationException();
        savedChunks = new();
        List<IChangeInfo> infos = new();
        target.ForEveryMember(member =>
        {
            if (member is not Layer layer)
                return;
            layer.LayerImage.EnqueueReplaceColor(oldColor, newColor);
            HashSet<VecI>? chunks = layer.LayerImage.FindAffectedChunks();
            CommittedChunkStorage storage = new(layer.LayerImage, chunks);
            savedChunks[layer.GuidValue] = storage;
            layer.LayerImage.CommitChanges();
            infos.Add(new LayerImageChunks_ChangeInfo(layer.GuidValue, chunks));
        });
        ignoreInUndo = !savedChunks.Any();
        return infos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (savedChunks is null)
            throw new InvalidOperationException();
        List<IChangeInfo> infos = new();
        target.ForEveryMember(member =>
        {
            if (member is not Layer layer)
                return;
            CommittedChunkStorage? storage = savedChunks[member.GuidValue];
            var chunks = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(layer.LayerImage, ref storage);
            infos.Add(new LayerImageChunks_ChangeInfo(layer.GuidValue, chunks));
        });
        savedChunks = null;
        return infos;
    }

    public override void Dispose()
    {
        if (savedChunks is null)
            return;
        foreach (var storage in savedChunks.Values)
            storage.Dispose();
    }
}
