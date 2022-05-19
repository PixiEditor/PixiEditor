using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class MaskChunks_ChangeInfo : IChangeInfo
{
    public Guid GuidValue { get; init; }
    public HashSet<VecI>? Chunks { get; init; }
}
