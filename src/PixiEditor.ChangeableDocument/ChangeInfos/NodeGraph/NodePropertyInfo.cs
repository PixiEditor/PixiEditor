using System.Diagnostics.CodeAnalysis;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodePropertyInfo(
    string PropertyName,
    string DisplayName,
    Type ValueType,
    bool IsInput,
    object? InputValue,
    Guid NodeId,
    IReadOnlyList<string> ConnectedProperties);
