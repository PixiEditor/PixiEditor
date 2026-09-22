namespace PixiEditor.GraphNavigation.Tests;

public sealed class TestInput(TestNode node, TestOutput? connectedOutput)
    : INavigableInputProperty<TestNode, TestInput, TestOutput>
{
    public TestNode Node { get; } = node;

    public TestOutput? ConnectedOutput { get; } = connectedOutput;
}