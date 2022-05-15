using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;
public record class SymmetryAxisPosition_ChangeInfo : IChangeInfo
{
    public SymmetryAxisDirection Direction { get; init; }
}
