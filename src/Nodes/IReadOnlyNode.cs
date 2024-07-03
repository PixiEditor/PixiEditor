namespace Nodes;

public interface IReadOnlyNode
{
    public string Name { get; }
    public IReadOnlyCollection<IInputProperty> InputProperties { get; }
    public IReadOnlyCollection<IOutputProperty> OutputProperties { get; }
    public IReadOnlyCollection<IReadOnlyNode> ConnectedNodes { get; }

    public void Execute(int frame);
    public bool Validate();
}
