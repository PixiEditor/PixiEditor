using System.Diagnostics.CodeAnalysis;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodePropertyInfo(
    string PropertyName,
    string DisplayName,
    Type ValueType,
    bool IsInput,
    object? InputValue,
    Guid NodeId);
