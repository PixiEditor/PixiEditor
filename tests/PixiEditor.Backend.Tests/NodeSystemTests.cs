using System.Reflection;
using Drawie.Backend.Core.Bridge;
using Drawie.Interop.VulkanAvalonia;
using Drawie.Skia;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Parser.Skia.Encoders;
using Xunit.Abstractions;

namespace PixiEditor.Backend.Tests;

public class NodeSystemTests
{
    private readonly ITestOutputHelper output;

    private Type[] knownNonSerializableTypes = new[]
    {
        typeof(Filter),
        typeof(Painter)
    };

    public NodeSystemTests(ITestOutputHelper output)
    {
        this.output = output;
        if (!DrawingBackendApi.HasBackend)
            DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(), new AvaloniaRenderingDispatcher());
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
            var endNode = NodeOperations.CreateNode(type.Value, target);

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
    private void TestThatAllInputsAreSerializableOrHaveFactories()
    {
        var allNodeTypes = GetNodeTypesWithoutPairs();
        var allFoundFactories = typeof(SerializationFactory).Assembly.GetTypes()
            .Where(x => x.IsAssignableTo(typeof(SerializationFactory))
                        && x is { IsAbstract: false, IsInterface: false }).ToList();
        
        List<SerializationFactory> factories = new();
        QoiEncoder encoder = new QoiEncoder();
        SerializationConfig config = new SerializationConfig(encoder);
        
        foreach (var factoryType in allFoundFactories)
        {
            var factory = (SerializationFactory)Activator.CreateInstance(factoryType);
            factories.Add(factory);
        }

        IReadOnlyDocument target = new MockDocument();

        foreach (var type in allNodeTypes)
        {
            var node = NodeOperations.CreateNode(type, target);
            Assert.NotNull(node);

            foreach (var input in node.InputProperties)
            {
                if (knownNonSerializableTypes.Contains(input.ValueType)) continue;
                if(input.ValueType.IsAbstract) continue;
                if (input.ValueType.IsAssignableTo(typeof(Delegate))) continue;
                bool hasFactory = factories.Any(x => x.OriginalType == input.ValueType);
                Assert.True(
                    input.ValueType.IsValueType || input.ValueType == typeof(string) || hasFactory, 
                    $"{input.ValueType} doesn't have a factory and is not serializable. Property: {input.InternalPropertyName}, NodeType: {node.GetType().Name}");
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