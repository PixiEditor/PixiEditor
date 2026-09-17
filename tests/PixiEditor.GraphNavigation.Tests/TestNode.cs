namespace PixiEditor.GraphNavigation.Tests;

public sealed class TestNode(string name) : INavigableNode<TestNode, TestInput, TestOutput>
{
    public string Name { get; } = name;

    public List<TestInput> Inputs { get; } = [];

    public List<TestOutput> Outputs { get; } = [];

    IEnumerable<TestInput> INavigableNode<TestNode, TestInput, TestOutput>.Inputs => Inputs;

    IEnumerable<TestOutput> INavigableNode<TestNode, TestInput, TestOutput>.Outputs => Outputs;
}