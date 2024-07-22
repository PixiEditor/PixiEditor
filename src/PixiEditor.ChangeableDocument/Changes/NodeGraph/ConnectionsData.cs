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
}
