namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class DeleteStructureMember_ChangeInfo(Guid Id, Guid ParentGuid) : IChangeInfo;
