using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;
public record class SymmetryAxisState_ChangeInfo(SymmetryAxisDirection Direction, bool State) : IChangeInfo;
