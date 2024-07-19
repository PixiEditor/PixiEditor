using System.Collections.Immutable;
using System.Reflection;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;

namespace PixiEditor.Backend.Tests;

public class NodeSystemTests
{
    public NodeSystemTests()
    {
        DrawingBackendApi.SetupBackend(new SkiaDrawingBackend());
    }

    [Fact]
    public void TestThatNodeGraphExecutesEmptyOutputNode()
    {
        NodeGraph graph = new NodeGraph();
        OutputNode outputNode = new OutputNode();

        graph.AddNode(outputNode);
        using RenderingContext context = new RenderingContext(0, VecI.Zero, ChunkResolution.Full, new VecI(1, 1));
        graph.Execute(context);

        Assert.Null(outputNode.CachedResult);
    }

    [Fact]
    public void TestThatCreateSimpleNodeDoesntThrow()
    {
        var allNodeTypes = typeof(Node).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(Node)) && x is { IsAbstract: false, IsInterface: false }).ToList();

        IReadOnlyDocument target = new MockDocument();

        foreach (var type in allNodeTypes)
        {
            if(type.GetCustomAttribute<PairNodeAttribute>() != null) continue;
            var node = NodeOperations.CreateNode(type, target);
            Assert.NotNull(node);
        }
    }
}