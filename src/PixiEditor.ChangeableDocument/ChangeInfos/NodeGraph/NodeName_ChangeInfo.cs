namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodeName_ChangeInfo(Guid NodeId, string NewName) : IChangeInfo;
