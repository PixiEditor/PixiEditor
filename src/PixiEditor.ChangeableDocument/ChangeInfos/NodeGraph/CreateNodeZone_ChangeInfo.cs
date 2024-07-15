namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNodeZone_ChangeInfo(Guid Id, Guid StartId, Guid EndId) : IChangeInfo;
