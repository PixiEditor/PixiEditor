using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.Structure;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNode_ChangeInfo(
    string InternalName,
    string NodeName,
    VecD Position,
    Guid Id,
    ImmutableArray<NodePropertyInfo> Inputs,
    ImmutableArray<NodePropertyInfo> Outputs,
    NodeMetadata? Metadata) : IChangeInfo
{

    public static ImmutableArray<NodePropertyInfo> CreatePropertyInfos(IEnumerable<INodeProperty> properties,
        bool isInput, Guid node)
    {
        return properties.Select(p => new NodePropertyInfo(p.InternalPropertyName, p.DisplayName, p.ValueType, isInput,
                GetNonOverridenValue(p),
                node,
                GetConnectedProperties(p)))
            .ToImmutableArray();
    }

    private static IReadOnlyList<(Guid, string)> GetConnectedProperties(INodeProperty nodeProperty)
    {
        List<(Guid, string)> connectedProperties = new();
        if (nodeProperty is IInputProperty inputProperty && inputProperty.Connection != null)
        {
            connectedProperties.Add((inputProperty.Connection.Node.Id, inputProperty.Connection.InternalPropertyName));
        }
        else if (nodeProperty is IOutputProperty outputProperty)
        {
            foreach (var connection in outputProperty.Connections)
            {
                connectedProperties.Add((connection.Node.Id, connection.InternalPropertyName));
            }
        }

        return connectedProperties;
    }

    public static CreateNode_ChangeInfo CreateFromNode(IReadOnlyNode node)
    {
        if (node is IReadOnlyStructureNode structureNode)
        {
            switch (structureNode)
            {
                case LayerNode layerNode:
                    return CreateLayer_ChangeInfo.FromLayer(layerNode);
                case FolderNode folderNode:
                    return CreateFolder_ChangeInfo.FromFolder(folderNode);
            }
        }

        string internalName = node.GetType().GetCustomAttribute<NodeInfoAttribute>()?.UniqueName;

        if (string.IsNullOrEmpty(internalName))
        {
            throw new ArgumentException(
                "Node does not have a unique name attribute. Please add [NodeInfo(\"UNIQUE_NAME\")] to the node class.");
        }

        Guid? pairNodeGuid = null;
        if (node is IPairNode pairNode)
        {
            pairNodeGuid = pairNode.OtherNode;
        }
        
        NodeMetadata metadata = new NodeMetadata(node) { PairNodeGuid = pairNodeGuid };

        return new CreateNode_ChangeInfo(internalName, node.DisplayName, node.Position,
            node.Id,
            CreatePropertyInfos(node.InputProperties, true, node.Id),
            CreatePropertyInfos(node.OutputProperties, false, node.Id), metadata);
    }

    private static object? GetNonOverridenValue(INodeProperty property) => property switch
    {
        IFuncInputProperty fieldProperty => fieldProperty.GetFuncConstantValue(),
        IInputProperty inputProperty => inputProperty.NonOverridenValue,
        _ => null
    };
}
