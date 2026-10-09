namespace PixiEditor.GraphNavigation.Tests;

public class TestOutput(TestNode node)
    : INavigableOutputProperty<TestNode, TestInput, TestOutput>
{
    public TestNode Node { get; } = node;

    public List<TestInput> ConnectedInputs { get; } = [];

    IEnumerable<TestInput> INavigableOutputProperty<TestNode, TestInput, TestOutput>.ConnectedInputs => ConnectedInputs;
}