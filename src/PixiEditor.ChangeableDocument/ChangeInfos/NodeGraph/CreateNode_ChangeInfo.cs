using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNode_ChangeInfo(string NodeName, VecD Position ,Guid Id) : IChangeInfo
{
}
