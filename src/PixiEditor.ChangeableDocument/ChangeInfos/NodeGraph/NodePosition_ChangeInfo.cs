using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodePosition_ChangeInfo(Guid NodeId, VecD NewPosition) : IChangeInfo;
