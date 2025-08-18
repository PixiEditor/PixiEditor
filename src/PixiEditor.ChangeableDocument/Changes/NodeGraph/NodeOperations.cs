using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Changes.Structure;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public static class NodeOperations
{
    private static Dictionary<Type, INodeFactory> allFactories;
    private static Dictionary<string, Type> nodeMap;

    static NodeOperations()
    {
        allFactories = new Dictionary<Type, INodeFactory>();
        var factoryTypes = typeof(Node).Assembly.GetTypes().Where(x =>
                x.IsAssignableTo(typeof(INodeFactory)) && x is { IsAbstract: false, IsInterface: false })
            .ToImmutableArray();
        foreach (var factoryType in factoryTypes)
        {
            INodeFactory factory = (INodeFactory)Activator.CreateInstance(factoryType);
            allFactories.Add(factory.NodeType, factory);
        }

        nodeMap = new Dictionary<string, Type>();
        var nodeTypes = typeof(Node).Assembly.GetTypes().Where(x =>
                x.IsSubclassOf(typeof(Node)) && x is { IsAbstract: false, IsInterface: false })
            .ToImmutableArray();

        foreach (var nodeType in nodeTypes)
        {
            NodeInfoAttribute? attribute = nodeType.GetCustomAttribute<NodeInfoAttribute>();
            if (attribute != null)
            {
                nodeMap.Add(attribute.UniqueName, nodeType);
            }
            else
            {
#if DEBUG
                throw new InvalidOperationException($"Node {nodeType.Name} does not have NodeInfoAttribute");
#endif
            }
        }
    }

    public static bool TryGetNodeType(string nodeUniqueName, out Type nodeType)
    {
        return nodeMap.TryGetValue(nodeUniqueName, out nodeType);
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

    public static List<IChangeInfo> AppendMember(Node parent, Node toAppend,
        out Dictionary<Guid, VecD> originalPositions)
    {
        InputProperty<Painter?>? parentInput =
            parent.GetInputProperty(OutputNode.InputPropertyName) as InputProperty<Painter?>;
        if (parentInput == null)
        {
            throw new InvalidOperationException("Parent node does not have an input property for appending members.");
        }

        OutputProperty<Painter>? toAddOutput = toAppend.GetOutputProperty("Output") as OutputProperty<Painter>;
        if (toAddOutput == null)
        {
            throw new InvalidOperationException("Node to append does not have an output property named 'Output'.");
        }

        InputProperty<Painter>? toAddInput =
            toAppend.GetInputProperty(OutputNode.InputPropertyName) as InputProperty<Painter>;

        if (toAddInput == null)
        {
            throw new InvalidOperationException(
                "Node to append does not have an input property for appending members.");
        }

        Guid memberId = toAppend.Id;

        List<IChangeInfo> changes = AppendMember(parentInput, toAddOutput, toAddInput, memberId);

        var adjustedPositions = AdjustPositionsAfterAppend(toAppend, parent,
            parentInput.Connection?.Node as Node ?? null, out originalPositions);

        changes.AddRange(adjustedPositions);
        return changes;
    }

    public static List<IChangeInfo> AppendMember(
        InputProperty<Painter?> parentInput,
        OutputProperty<Painter> toAddOutput,
        InputProperty<Painter> toAddInput, Guid memberId)
    {
        List<IChangeInfo> changes = new();
        IOutputProperty? previouslyConnected = null;

        if(parentInput == null) return changes;

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

    public static List<IChangeInfo> AdjustPositionsAfterAppend(Node member, Node appendedTo, Node? previouslyConnected,
        out Dictionary<Guid, VecD> originalPositions)
    {
        List<IChangeInfo> changes = new();
        Dictionary<Guid, VecD> originalPositionDict = new();

        member.Position = new VecD(appendedTo.Position.X - 250, appendedTo.Position.Y);

        changes.Add(new NodePosition_ChangeInfo(member.Id, member.Position));

        previouslyConnected?.TraverseBackwards((aNode, previousNode, _) =>
        {
            if (aNode is Node toMove)
            {
                originalPositionDict.Add(toMove.Id, toMove.Position);
                var y = toMove.Position.Y;
                toMove.Position = (previousNode?.Position ?? member.Position) - new VecD(250, 0);
                toMove.Position = new VecD(toMove.Position.X, y);
                changes.Add(new NodePosition_ChangeInfo(toMove.Id, toMove.Position));
            }

            return true;
        });

        originalPositions = originalPositionDict;
        return changes;
    }

    public static List<IChangeInfo> AdjustPositionsBeforeAppend(Node member, Node appendedTo,
        out Dictionary<Guid, VecD> originalPositions)
    {
        List<IChangeInfo> changes = new();
        Dictionary<Guid, VecD> originalPositionDict = new();

        member.TraverseBackwards((aNode, previousNode, _) =>
        {
            if (aNode is Node toMove)
            {
                originalPositionDict.Add(toMove.Id, toMove.Position);
                var y = toMove.Position.Y;
                VecD pos = member.Position + new VecD(250, 0);
                if (previousNode != null)
                {
                    pos = previousNode.Position - new VecD(250, 0);
                }

                toMove.Position = pos;
                toMove.Position = new VecD(toMove.Position.X, y);
                changes.Add(new NodePosition_ChangeInfo(toMove.Id, toMove.Position));

                if (aNode == appendedTo) return false;
            }

            return true;
        });

        member.Position = new VecD(appendedTo.Position.X - 250, appendedTo.Position.Y);
        changes.Add(new NodePosition_ChangeInfo(member.Id, member.Position));

        originalPositions = originalPositionDict;
        return changes;
    }

    public static List<IChangeInfo> RevertPositions(Dictionary<Guid, VecD> positions, IReadOnlyDocument target)
    {
        List<IChangeInfo> changes = new();
        foreach (var (guid, position) in positions)
        {
            var node = target.FindNode(guid) as Node;
            if (node == null) continue;

            node.Position = position;
            changes.Add(new NodePosition_ChangeInfo(guid, position));
        }

        return changes;
    }

    public static ConnectionsData CreateConnectionsData(IReadOnlyNode node)
    {
        var originalOutputConnections = new Dictionary<PropertyConnection, List<PropertyConnection>>();

        foreach (var outputProp in node.OutputProperties)
        {
            PropertyConnection outputConnection = new(outputProp.Node.Id, outputProp.InternalPropertyName);
            originalOutputConnections.Add(outputConnection, new List<PropertyConnection>());
            foreach (var conn in outputProp.Connections)
            {
                originalOutputConnections[outputConnection]
                    .Add(new PropertyConnection(conn.Node.Id, conn.InternalPropertyName));
            }
        }

        var originalInputConnections = node.InputProperties.Select(x =>
                (new PropertyConnection(x.Node.Id, x.InternalPropertyName),
                    new PropertyConnection(x.Connection?.Node.Id, x.Connection?.InternalPropertyName)))
            .ToList();

        return new ConnectionsData(originalOutputConnections, originalInputConnections);
    }

    public static List<IChangeInfo> ConnectStructureNodeProperties(ConnectionsData originalConnections, Node node,
        IReadOnlyNodeGraph graph)
    {
        if (node == null || originalConnections == null || graph == null)
        {
            return new List<IChangeInfo>();
        }

        List<IChangeInfo> changes = new();
        foreach (var connections in originalConnections.originalOutputConnections)
        {
            PropertyConnection outputConnection = connections.Key;
            if (outputConnection == null)
                continue;

            IOutputProperty outputProp = node.GetOutputProperty(outputConnection.PropertyName);

            if (outputProp == null)
            {
                continue;
            }

            foreach (var connection in connections.Value)
            {
                var inputNode = graph.AllNodes.FirstOrDefault(x => x.Id == connection.NodeId);
                if (inputNode is null)
                    continue;

                IInputProperty property = inputNode.GetInputProperty(connection.PropertyName);
                if (property is null)
                    continue;

                outputProp.ConnectTo(property);
                changes.Add(new ConnectProperty_ChangeInfo(node.Id, property.Node.Id, outputProp.InternalPropertyName,
                    property.InternalPropertyName));
            }
        }

        foreach (var connection in originalConnections.originalInputConnections)
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

    public static List<IChangeInfo> DetachNode(Changeables.Graph.NodeGraph target, Node? node)
    {
        List<IChangeInfo> changes = new();
        if (node == null)
        {
            return changes;
        }

        foreach (var input in node.InputProperties)
        {
            if (input.Connection == null)
            {
                continue;
            }

            input.Connection.DisconnectFrom(input);
            changes.Add(new ConnectProperty_ChangeInfo(null, node.Id, null, input.InternalPropertyName));
        }

        foreach (var output in node.OutputProperties)
        {
            foreach (var input in output.Connections.ToArray())
            {
                output.DisconnectFrom(input);
                changes.Add(new ConnectProperty_ChangeInfo(null, input.Node.Id, null, input.InternalPropertyName));
            }
        }

        return changes;
    }

    public static List<IChangeInfo> CreateUpdateInputs(Node copy)
    {
        List<IChangeInfo> changes = new();
        foreach (var input in copy.InputProperties)
        {
            object value = input.NonOverridenValue;
            if (value is Delegate del)
            {
                value = del.DynamicInvoke(FuncContext.NoContext);
                if (value is ShaderExpressionVariable expressionVariable)
                {
                    value = expressionVariable.GetConstant();
                }
            }

            changes.Add(new PropertyValueUpdated_ChangeInfo(copy.Id, input.InternalPropertyName, value));
        }

        return changes;
    }
}

public record PropertyConnection(Guid? NodeId, string? PropertyName);
