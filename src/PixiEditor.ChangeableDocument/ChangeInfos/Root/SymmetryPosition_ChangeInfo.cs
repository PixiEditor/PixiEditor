using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;
public record class SymmetryPosition_ChangeInfo : IChangeInfo
{
    public SymmetryDirection Direction { get; init; }
}
