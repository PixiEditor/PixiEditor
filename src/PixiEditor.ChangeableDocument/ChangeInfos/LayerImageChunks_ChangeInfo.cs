using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.ChangeInfos;

public record class LayerImageChunks_ChangeInfo : IChangeInfo
{
    public Guid LayerGuid { get; init; }
    public HashSet<Vector2i>? Chunks { get; init; }
}
