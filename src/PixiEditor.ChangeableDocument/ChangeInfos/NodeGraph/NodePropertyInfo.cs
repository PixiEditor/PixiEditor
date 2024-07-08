namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodePropertyInfo(
    string PropertyName,
    string DisplayName,
    Type ValueType,
    bool IsInput,
    Guid NodeId);
