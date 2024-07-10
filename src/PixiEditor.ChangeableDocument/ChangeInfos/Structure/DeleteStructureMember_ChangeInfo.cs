using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class DeleteStructureMember_ChangeInfo(Guid Id, Guid ParentGuid) : DeleteNode_ChangeInfo(Id);
