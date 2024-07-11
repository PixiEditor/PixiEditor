using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public static class NodeOperations
{
    public static List<ConnectProperty_ChangeInfo> AppendMember(InputProperty<ChunkyImage?> parentInput,
        OutputProperty<ChunkyImage> toAddOutput,
        InputProperty<ChunkyImage> toAddInput, Guid memberId)
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

    public static List<IChangeInfo> ConnectStructureNodeProperties(List<IInputProperty> originalOutputConnections,
        List<(IInputProperty, IOutputProperty?)> originalInputConnections, StructureNode node)
    {
        List<IChangeInfo> changes = new();
        foreach (var connection in originalOutputConnections)
        {
            node.Output.ConnectTo(connection);
            changes.Add(new ConnectProperty_ChangeInfo(node.Id, connection.Node.Id, node.Output.InternalPropertyName,
                connection.InternalPropertyName));
        }

        foreach (var connection in originalInputConnections)
        {
            if (connection.Item2 is null)
                continue;

            IInputProperty? input =
                node.InputProperties.FirstOrDefault(
                    x => x.InternalPropertyName == connection.Item1.InternalPropertyName);

            if (input != null)
            {
                connection.Item2.ConnectTo(input);
                changes.Add(new ConnectProperty_ChangeInfo(connection.Item2.Node.Id, node.Id,
                    connection.Item2.InternalPropertyName,
                    input.InternalPropertyName));
            }
        }

        return changes;
    }
}
