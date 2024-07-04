namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public interface INodeGraph
{
    public IReadOnlyCollection<IReadOnlyNode> AllNodes { get; }
    public IReadOnlyNode OutputNode { get; }
    public void AddNode(IReadOnlyNode node);
    public void RemoveNode(IReadOnlyNode node);
}
