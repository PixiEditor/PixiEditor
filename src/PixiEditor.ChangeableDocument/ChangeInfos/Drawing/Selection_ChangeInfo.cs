using ChunkyImageLib.DataHolders;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class Selection_ChangeInfo : IChangeInfo
{
    public HashSet<Vector2i>? Chunks { get; init; }
}
