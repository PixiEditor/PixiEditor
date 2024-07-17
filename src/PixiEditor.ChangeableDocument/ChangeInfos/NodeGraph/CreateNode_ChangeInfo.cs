using System.Collections;
using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNode_ChangeInfo(
    string InternalName,
    string NodeName,
    VecD Position,
    Guid Id,
    ImmutableArray<NodePropertyInfo> Inputs,
    ImmutableArray<NodePropertyInfo> Outputs) : IChangeInfo
{
    public static ImmutableArray<NodePropertyInfo> CreatePropertyInfos(IEnumerable<INodeProperty> properties,
        bool isInput, Guid guid)
    {
        return properties.Select(p => new NodePropertyInfo(p.InternalPropertyName, p.DisplayName, p.ValueType, isInput, GetNonOverridenValue(p), guid))
            .ToImmutableArray();
    }

    public static CreateNode_ChangeInfo CreateFromNode(IReadOnlyNode node) =>
        CreateFromNode(node, node.GetType().Name.Replace("Node", ""));
    
    public static CreateNode_ChangeInfo CreateFromNode(IReadOnlyNode node, string name)
    {
        return new CreateNode_ChangeInfo(node.InternalName, name, node.Position,
            node.Id,
            CreatePropertyInfos(node.InputProperties, true, node.Id), CreatePropertyInfos(node.OutputProperties, false, node.Id));
    }

    private static object? GetNonOverridenValue(INodeProperty property) => property switch
    {
        IFieldInputProperty fieldProperty => fieldProperty.GetFieldConstantValue(),
        IInputProperty inputProperty => inputProperty.NonOverridenValue,
        _ => null
    };
}
