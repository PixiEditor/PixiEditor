namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNodeZone_ChangeInfo(Guid Id, string internalName, Guid StartId, Guid EndId) : IChangeInfo;
