namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class DeleteStructureMember_ChangeInfo(Guid GuidValue, Guid ParentGuid) : IChangeInfo;
