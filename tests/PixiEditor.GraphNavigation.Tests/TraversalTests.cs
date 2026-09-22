namespace PixiEditor.GraphNavigation.Tests;

public class TraversalTests
{
    [Fact]
    public void NodeNavigator_Forwards_EnumeratesBreadthFirst()
    {
        var root = new TestNode("A");
        var b = new TestNode("B");
        var c = new TestNode("C");
        var d = new TestNode("D");
        var e = new TestNode("E");

        Link(root, b);
        Link(root, c);
        Link(b, d);
        Link(c, e);

        var visited = new NodeNavigator<TestNode, TestInput, TestOutput>(root)
            .Forwards()
            .Select(ctx => ctx.Current.Name)
            .ToList();

        Assert.Equal(["A", "B", "C", "D", "E"], visited);
    }

    [Fact]
    public void NodeNavigator_Forwards_WhenYieldOriginIsFalse_ExcludesOrigin()
    {
        var root = new TestNode("A");
        var b = new TestNode("B");
        var c = new TestNode("C");
        var d = new TestNode("D");
        var e = new TestNode("E");

        Link(root, b);
        Link(root, c);
        Link(b, d);
        Link(c, e);

        var visited = new NodeNavigator<TestNode, TestInput, TestOutput>(root)
            .Forwards(yieldOrigin: false)
            .Select(ctx => ctx.Current.Name)
            .ToList();

        Assert.Equal(["B", "C", "D", "E"], visited);
    }

    [Fact]
    public void NodeNavigator_Backwards_RespectsBrancherFlowControl()
    {
        var root = new TestNode("A");
        var b = new TestNode("B");
        var c = new TestNode("C");
        var d = new TestNode("D");
        var e = new TestNode("E");

        Link(b, root);
        Link(c, root);
        Link(d, b);
        Link(e, c);

        var visited = new NodeNavigator<TestNode, TestInput, TestOutput>(root)
            .Backwards(ctx =>
            {
                if (ctx.Current == root)
                {
                    return Traverse.Continue;
                }

                if (ctx.Current == b)
                {
                    return Traverse.SkipChildren;
                }

                if (ctx.Current == c)
                {
                    return Traverse.ExitInclusive;
                }

                return Traverse.Continue;
            })
            .Select(ctx => ctx.Current.Name)
            .ToList();

        Assert.Equal(["A", "B", "C"], visited);
    }

    [Fact]
    public void NodeTraversalEngine_Forwards_VisitsEachNodeOnceInBreadthFirstOrder()
    {
        var root = new TestNode("A");
        var b = new TestNode("B");
        var c = new TestNode("C");
        var d = new TestNode("D");
        var e = new TestNode("E");

        Link(root, b);
        Link(root, c);
        Link(b, d);
        Link(c, e);

        var engine = new NodeTraversalEngine<TestNode, TestInput, TestOutput>(root);
        var visited = new List<string>();

        while (engine.TryGetNext(out var context))
        {
            visited.Add(context.Current.Name);
            engine.ExpandForwards(context);
        }

        Assert.Equal(["A", "B", "C", "D", "E"], visited);
    }

    [Fact]
    public void NodeTraversalEngine_Backwards_VisitsEachNodeOnceInBreadthFirstOrder()
    {
        var root = new TestNode("A");
        var b = new TestNode("B");
        var c = new TestNode("C");
        var d = new TestNode("D");
        var e = new TestNode("E");

        Link(b, root);
        Link(c, root);
        Link(d, b);
        Link(e, c);

        var engine = new NodeTraversalEngine<TestNode, TestInput, TestOutput>(root);
        var visited = new List<string>();

        while (engine.TryGetNext(out var context))
        {
            visited.Add(context.Current.Name);
            engine.ExpandBackwards(context);
        }

        Assert.Equal(["A", "B", "C", "D", "E"], visited);
    }

    private static void Link(TestNode source, TestNode target)
    {
        var output = new TestOutput(source);
        var input = new TestInput(target, output);

        source.Outputs.Add(output);
        target.Inputs.Add(input);

        output.ConnectedInputs.Add(input);
    }
}
