namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public class ConnectionsData
{
    public Dictionary<PropertyConnection, List<PropertyConnection>> originalOutputConnections = new();
    public List<(PropertyConnection input, PropertyConnection? output)> originalInputConnections = new();

    public ConnectionsData(Dictionary<PropertyConnection, List<PropertyConnection>> originalOutputConnections,
        List<(PropertyConnection, PropertyConnection?)> originalInputConnections)
    {
        this.originalOutputConnections = originalOutputConnections;
        this.originalInputConnections = originalInputConnections;
    }

    public ConnectionsData WithUpdatedIds(Dictionary<Guid, Guid> nodeMap)
    {
        Dictionary<PropertyConnection, List<PropertyConnection>> newOutputConnections = new();
        foreach (var (key, value) in originalOutputConnections)
        {
            Guid? sourceNodeId = key.NodeId;
            if (sourceNodeId.HasValue)
            {
                if (!nodeMap.TryGetValue(sourceNodeId.Value, out var newSourceNodeId))
                    continue;

                sourceNodeId = newSourceNodeId;
            }

            var valueCopy = new List<PropertyConnection>();
            foreach (var connection in value)
            {
                Guid? targetNodeId = connection.NodeId;
                if (targetNodeId.HasValue)
                {
                    if (!nodeMap.TryGetValue(targetNodeId.Value, out var targetId))
                    {
                        continue;
                    }

                    targetNodeId = targetId;
                }

                valueCopy.Add(connection with { NodeId = targetNodeId });
            }

            newOutputConnections.Add(key with { NodeId = sourceNodeId }, valueCopy);
        }

        List<(PropertyConnection, PropertyConnection?)> newInputConnections = new();
        foreach (var (input, output) in originalInputConnections)
        {
            Guid? inputNodeId = input.NodeId;
            if (inputNodeId.HasValue)
            {
                if (!nodeMap.TryGetValue(inputNodeId.Value, out var newInputNodeId))
                    continue;

                inputNodeId = newInputNodeId;
            }

            Guid? outputNodeId = output?.NodeId;
            if (outputNodeId.HasValue)
            {
                if (!nodeMap.TryGetValue(outputNodeId.Value, out var newOutputNodeId))
                    continue;

                outputNodeId = newOutputNodeId;
            }

            newInputConnections.Add((input with { NodeId = inputNodeId },
                new PropertyConnection(outputNodeId, output?.PropertyName)));
        }

        return new ConnectionsData(newOutputConnections, newInputConnections);
    }
}
