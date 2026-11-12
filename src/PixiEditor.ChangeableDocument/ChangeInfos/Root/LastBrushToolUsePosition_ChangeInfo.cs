using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;

public record class LastBrushToolUsePosition_ChangeInfo(VecD? newValue) : IChangeInfo;
