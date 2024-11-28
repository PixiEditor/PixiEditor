using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal class ReplaceColor_Change : Change
{
    private readonly Color oldColor;
    private readonly Color newColor;

    private Dictionary<Guid, CommittedChunkStorage>? savedChunks;
    private int frame;

    [GenerateMakeChangeAction]
    public ReplaceColor_Change(Color oldColor, Color newColor, int frame)
    {
        this.oldColor = oldColor;
        this.newColor = newColor;
        this.frame = frame;
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
            if (member is not ImageLayerNode layer)
                return;
            //TODO: Add support for replacing in different Layer types
            var layerImage = layer.GetLayerImageAtFrame(frame);
            layerImage.EnqueueReplaceColor(oldColor, newColor);
            var affArea = layerImage.FindAffectedArea();
            CommittedChunkStorage storage = new(layerImage, affArea.Chunks);
            savedChunks[layer.Id] = storage;
            layerImage.CommitChanges();
            infos.Add(new LayerImageArea_ChangeInfo(layer.Id, affArea));
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
            if (member is not ImageLayerNode layer)
                return;
            CommittedChunkStorage? storage = savedChunks[member.Id];
            var affArea = DrawingChangeHelper.ApplyStoredChunksDisposeAndSetToNull(layer.GetLayerImageAtFrame(frame), ref storage);
            infos.Add(new LayerImageArea_ChangeInfo(layer.Id, affArea));
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
