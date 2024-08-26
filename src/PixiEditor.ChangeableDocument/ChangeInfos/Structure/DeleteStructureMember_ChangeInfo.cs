using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class DeleteStructureMember_ChangeInfo(Guid Id) : DeleteNode_ChangeInfo(Id);
