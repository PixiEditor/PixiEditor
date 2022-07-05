using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;
public record class SymmetryAxisPosition_ChangeInfo(SymmetryAxisDirection Direction, int NewPosition) : IChangeInfo;
