using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class LayerImageChunks_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
    public HashSet<VecI>? Chunks { get; init; }
}
