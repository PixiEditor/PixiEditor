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

    public override bool InitializeAndValidate(Document target)
    {
        return true;
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
            var affArea = layer.LayerImage.FindAffectedArea();
            CommittedChunkStorage storage = new(layer.LayerImage, affArea.Chunks);
            savedChunks[layer.GuidValue] = storage;
            layer.LayerImage.CommitChanges();
            infos.Add(new LayerImageArea_ChangeInfo(layer.GuidValue, affArea));
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
            var affArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(layer.LayerImage, ref storage);
            infos.Add(new LayerImageArea_ChangeInfo(layer.GuidValue, affArea));
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
