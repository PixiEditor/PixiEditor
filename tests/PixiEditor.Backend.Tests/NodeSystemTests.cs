using System.Collections.Immutable;
using System.Reflection;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using PixiEditor.Parser.Skia.Encoders;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PixiEditor.Backend.Tests;

public class NodeSystemTests
{
    private readonly ITestOutputHelper output;

    public NodeSystemTests(ITestOutputHelper output)
    {
        this.output = output;
        if (!DrawingBackendApi.HasBackend)
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
            .Where(x =>
                x.IsAssignableTo(typeof(Node))
                && x is { IsAbstract: false, IsInterface: false }
                && x.GetCustomAttribute<PairNodeAttribute>() == null).ToList();

        IReadOnlyDocument target = new MockDocument();

        foreach (var type in allNodeTypes)
        {
            var node = NodeOperations.CreateNode(type, target);
            Assert.NotNull(node);
        }
    }

    [Fact]
    public void TestThatCreatePairNodeDoesntThrow()
    {
        var allNodeTypes = typeof(Node).Assembly.GetTypes()
            .Where(x =>
                x.IsAssignableTo(typeof(Node))
                && x is { IsAbstract: false, IsInterface: false }
                && x.GetCustomAttribute<PairNodeAttribute>() != null).ToList();

        IReadOnlyDocument target = new MockDocument();

        Dictionary<Type, Type> pairs = new();

        for (var i = 0; i < allNodeTypes.Count; i++)
        {
            var type = allNodeTypes[i];
            var pairAttribute = type.GetCustomAttribute<PairNodeAttribute>();

            if (pairAttribute == null) continue;

            if (!pairAttribute.IsStartingType) continue;

            pairs[type] = pairAttribute.OtherType;
        }

        foreach (var type in pairs)
        {
            var startNode = NodeOperations.CreateNode(type.Key, target);
            var endNode = NodeOperations.CreateNode(type.Value, target, startNode);

            Assert.NotNull(startNode);
            Assert.NotNull(endNode);
        }
    }

    [Fact]
    public void TestThatSerializeNodeDoesntThrow()
    {
        var allNodeTypes = GetNodeTypesWithoutPairs();

        IReadOnlyDocument target = new MockDocument();

        foreach (var type in allNodeTypes)
        {
            var node = NodeOperations.CreateNode(type, target);
            Assert.NotNull(node);

            Dictionary<string, object> data = new Dictionary<string, object>();
            node.SerializeAdditionalData(data);
            Assert.NotNull(data);
        }
    }

    [Fact]
    private void TestThatAllInputsAreSerializable()
    {
        var allNodeTypes = GetNodeTypesWithoutPairs();

        IReadOnlyDocument target = new MockDocument();

        foreach (var type in allNodeTypes)
        {
            var node = NodeOperations.CreateNode(type, target);
            Assert.NotNull(node);

            QoiEncoder encoder = new QoiEncoder();

            foreach (var input in node.InputProperties)
            {
                object? defaultOfType = Activator.CreateInstance(input.ValueType);
                object serialized = SerializationUtil.SerializeObject(defaultOfType, encoder);
                Assert.NotNull(serialized);
            }
        }
    }

    private static List<Type> GetNodeTypesWithoutPairs()
    {
        var allNodeTypes = typeof(Node).Assembly.GetTypes()
            .Where(x =>
                x.IsAssignableTo(typeof(Node))
                && x is { IsAbstract: false, IsInterface: false }
                && x.GetCustomAttribute<PairNodeAttribute>() == null).ToList();
        return allNodeTypes;
    }
}