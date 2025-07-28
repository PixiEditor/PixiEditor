namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

public class ConnectionsData
{
    public Dictionary<PropertyConnection, List<PropertyConnection>> originalOutputConnections = new();
    public List<(PropertyConnection input, PropertyConnection? output)> originalInputConnections = new();
    
    public ConnectionsData(Dictionary<PropertyConnection, List<PropertyConnection>> originalOutputConnections, List<(PropertyConnection, PropertyConnection?)> originalInputConnections)
    {
        this.originalOutputConnections = originalOutputConnections;
        this.originalInputConnections = originalInputConnections;
    }

    public ConnectionsData WithUpdatedIds(Dictionary<Guid,Guid> nodeMap)
    {
        Dictionary<PropertyConnection, List<PropertyConnection>> newOutputConnections = new();
        foreach (var (key, value) in originalOutputConnections)
        {
            Guid? sourceNodeId = key.NodeId;
            if (sourceNodeId.HasValue)
            {
                sourceNodeId = nodeMap[sourceNodeId.Value];
            }
            
            var valueCopy = new List<PropertyConnection>();
            foreach (var connection in value)
            {
                Guid? targetNodeId = connection.NodeId;
                if (targetNodeId.HasValue)
                {
                    targetNodeId = nodeMap[targetNodeId.Value];
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
                inputNodeId = nodeMap[inputNodeId.Value];
            }
            
            Guid? outputNodeId = output?.NodeId;
            if (outputNodeId.HasValue)
            {
                outputNodeId = nodeMap[outputNodeId.Value];
            }
            
            newInputConnections.Add((input with { NodeId = inputNodeId }, new PropertyConnection(outputNodeId, output?.PropertyName)));
        }
        
        return new ConnectionsData(newOutputConnections, newInputConnections);
    }
}
