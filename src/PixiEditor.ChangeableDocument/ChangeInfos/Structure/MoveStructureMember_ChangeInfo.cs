namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class MoveStructureMember_ChangeInfo(Guid GuidValue, Guid ParentFromGuid, Guid ParentToGuid, int NewIndex) : IChangeInfo;
