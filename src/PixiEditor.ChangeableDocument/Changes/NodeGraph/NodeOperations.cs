using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Changes.Structure;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public static class NodeOperations
{
    private static Dictionary<Type, INodeFactory> allFactories;

    static NodeOperations()
    {
        allFactories = new Dictionary<Type, INodeFactory>();
        var factoryTypes = typeof(Node).Assembly.GetTypes().Where(x =>
            x.IsAssignableTo(typeof(INodeFactory)) && x is { IsAbstract: false, IsInterface: false }).ToImmutableArray();
        foreach (var factoryType in factoryTypes)
        {
            INodeFactory factory = (INodeFactory)Activator.CreateInstance(factoryType);
            allFactories.Add(factory.NodeType, factory);
        }
    }

    public static Node CreateNode(Type nodeType, IReadOnlyDocument target, params object[] optionalParameters)
    {
        Node node = null;
        if (allFactories.TryGetValue(nodeType, out INodeFactory factory))
        {
            node = factory.CreateNode(target);
        }
        else
        {
            node = (Node)Activator.CreateInstance(nodeType, optionalParameters);
        }
        
        return node;
    }

    public static List<ConnectProperty_ChangeInfo> AppendMember(InputProperty<Surface?> parentInput,
        OutputProperty<Surface> toAddOutput,
        InputProperty<Surface> toAddInput, Guid memberId)
    {
        List<ConnectProperty_ChangeInfo> changes = new();
        IOutputProperty? previouslyConnected = null;
        if (parentInput.Connection != null)
        {
            previouslyConnected = parentInput.Connection;
        }

        toAddOutput.ConnectTo(parentInput);

        if (previouslyConnected != null)
        {
            previouslyConnected.ConnectTo(toAddInput);
            changes.Add(new ConnectProperty_ChangeInfo(previouslyConnected.Node.Id, memberId,
                previouslyConnected.InternalPropertyName, toAddInput.InternalPropertyName));
        }

        changes.Add(new ConnectProperty_ChangeInfo(memberId, parentInput.Node.Id,
            toAddOutput.InternalPropertyName, parentInput.InternalPropertyName));

        return changes;
    }

    public static List<IChangeInfo> DetachStructureNode(StructureNode structureNode)
    {
        List<IChangeInfo> changes = new();

        if (structureNode.Background.Connection != null)
        {
            // connect connection to next input if possible

            var connections = structureNode.Output.Connections.ToArray();

            var output = structureNode.Background.Connection;

            foreach (var input in connections)
            {
                output.ConnectTo(input);
                changes.Add(new ConnectProperty_ChangeInfo(output.Node.Id, input.Node.Id,
                    output.InternalPropertyName, input.InternalPropertyName));
            }

            structureNode.Background.Connection.DisconnectFrom(structureNode.Background);
            changes.Add(new ConnectProperty_ChangeInfo(null, structureNode.Id, null,
                structureNode.Background.InternalPropertyName));
        }

        var outputs = structureNode.Output.Connections.ToArray();
        foreach (var outputConnection in outputs)
        {
            structureNode.Output.DisconnectFrom(outputConnection);
            changes.Add(new ConnectProperty_ChangeInfo(null, outputConnection.Node.Id, null,
                outputConnection.InternalPropertyName));
        }

        return changes;
    }

    public static List<IChangeInfo> ConnectStructureNodeProperties(
        List<PropertyConnection> originalOutputConnections,
        List<(PropertyConnection, PropertyConnection?)> originalInputConnections, StructureNode node,
        IReadOnlyNodeGraph graph)
    {
        List<IChangeInfo> changes = new();
        foreach (var connection in originalOutputConnections)
        {
            var inputNode = graph.AllNodes.FirstOrDefault(x => x.Id == connection.NodeId);
            IInputProperty property = inputNode.GetInputProperty(connection.PropertyName);
            node.Output.ConnectTo(property);
            changes.Add(new ConnectProperty_ChangeInfo(node.Id, property.Node.Id, node.Output.InternalPropertyName,
                property.InternalPropertyName));
        }

        foreach (var connection in originalInputConnections)
        {
            var outputNode = graph.AllNodes.FirstOrDefault(x => x.Id == connection.Item2?.NodeId);

            if (outputNode is null)
                continue;

            IOutputProperty output = outputNode.GetOutputProperty(connection.Item2.PropertyName);

            if (output is null)
                continue;

            IInputProperty? input =
                node.GetInputProperty(connection.Item1.PropertyName);

            if (input != null)
            {
                output.ConnectTo(input);
                changes.Add(new ConnectProperty_ChangeInfo(output.Node.Id, node.Id,
                    output.InternalPropertyName,
                    input.InternalPropertyName));
            }
        }

        return changes;
    }
}

public record PropertyConnection(Guid? NodeId, string? PropertyName);
