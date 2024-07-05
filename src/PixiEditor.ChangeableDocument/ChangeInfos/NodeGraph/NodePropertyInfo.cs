namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodePropertyInfo(
    string Name,
    Type ValueType,
    bool IsInput,
    Guid NodeId);
