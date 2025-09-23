using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Tests;

namespace PixiEditor.Backend.Tests;

public class RepeatNodeTests : PixiEditorTest
{
    [Fact]
    public void TestThatRepeatNodeProperlyDuplicatesNodes()
    {
        RepeatNodeStart start = new();
        RepeatNodeEnd end = new();
        MathNode mathNode = new();

        mathNode.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };
        start.Output.ConnectTo(mathNode.Y);

        mathNode.Result.ConnectTo(end.Input);
        start.Iterations.NonOverridenValue = 3;

        RenderContext context = new(null, new KeyFrameTime(), ChunkResolution.Full, VecI.Zero, VecI.Zero,
            ColorSpace.CreateSrgb(), SamplingOptions.Bilinear);
        var executionQueue = GraphUtils.CalculateExecutionQueue(end, true);
        foreach (var node in executionQueue)
        {
            node.Execute(context);
        }

        Assert.True(end.Output.Value is Delegate);
        var func = (Delegate)end.Output.Value!;
        var result = func.DynamicInvoke(ShaderFuncContext.NoContext);
        Assert.Equal(3, ((Float1)result!).ConstantValue);
    }

    [Fact]
    public void TestThatNestedRepeatNodesProperlyExecutes()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        mathNode.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };
        startInner.Output.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(endInner.Input);

        startOuter.Output.ConnectTo(startInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        startOuter.Iterations.NonOverridenValue = 2;
        startInner.Iterations.NonOverridenValue = 7;

        RenderContext context = new(null, new KeyFrameTime(), ChunkResolution.Full, VecI.Zero, VecI.Zero,
            ColorSpace.CreateSrgb(), SamplingOptions.Bilinear);
        var executionQueue = GraphUtils.CalculateExecutionQueue(endOuter, true);
        foreach (var node in executionQueue)
        {
            node.Execute(context);
        }

        Assert.True(endOuter.Output.Value is Delegate);
        var func = (Delegate)endOuter.Output.Value!;
        var result = func.DynamicInvoke(ShaderFuncContext.NoContext);
        Assert.Equal(14, ((Float1)result!).ConstantValue);
    }

    [Fact]
    public void TestThatNodePairingWorksForMultipleNestingLevels()
    {
        RepeatNodeStart start1 = new();
        RepeatNodeEnd end1 = new();

        RepeatNodeStart start2 = new();
        RepeatNodeEnd end2 = new();

        start1.Output.ConnectTo(start2.Input);
        start2.Output.ConnectTo(end2.Input);
        end2.Output.ConnectTo(end1.Input);

        var emptyContext = new RenderContext(null, new KeyFrameTime(), ChunkResolution.Full, VecI.Zero, VecI.Zero,
            ColorSpace.CreateSrgb(), SamplingOptions.Bilinear);
        start1.Execute(emptyContext);
        start2.Execute(emptyContext);
        end2.Execute(emptyContext);
        end1.Execute(emptyContext);

        Assert.Equal(start1.OtherNode, end1.Id);
        Assert.Equal(end1.OtherNode, start1.Id);
        Assert.Equal(start2.OtherNode, end2.Id);
        Assert.Equal(end2.OtherNode, start2.Id);
    }

    [Fact]
    public void CalculateHandledNodesProperlyReportsMathNodeOnly()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        mathNode.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };
        startInner.Output.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(endInner.Input);

        startOuter.Output.ConnectTo(startInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Single(endInner.HandledNodes);
        Assert.Contains(mathNode, endOuter.HandledNodes);

        Assert.Equal(3, endOuter.HandledNodes.Count); // mathNode, startInner, endInner
        Assert.Contains(startInner, endOuter.HandledNodes);
        Assert.Contains(endInner, endOuter.HandledNodes);
    }

    [Fact]
    public void FindStartNodeProperlyFindsNodeInNestedZone()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        startOuter.Output.ConnectTo(startInner.Input);
        startInner.Output.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(endInner.Input);

        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(startOuter, endOuter.FindStartNode(out _));
        Assert.Equal(startInner, endInner.FindStartNode(out _));
    }


    [Fact]
    public void FindStartNodeProperlyFindsNodeInNestedZoneWithOuterZoneReference()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        startInner.Output.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(endInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNode.X);

        startOuter.Output.ConnectTo(startInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(startOuter, endOuter.FindStartNode(out _));
        Assert.Equal(startInner, endInner.FindStartNode(out _));
    }

    [Fact]
    public void FindStartNodeProperlyFindsNodeInNestedZoneWithOuterZoneReference2()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();
        MathNode mathNode2 = new();

        startInner.Output.ConnectTo(mathNode.Y);
        startInner.CurrentIteration.ConnectTo(mathNode2.X);
        startOuter.CurrentIteration.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(mathNode2.Y);

        mathNode2.Result.ConnectTo(endInner.Input);

        startOuter.Output.ConnectTo(startInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(startOuter, endOuter.FindStartNode(out _));
        Assert.Equal(startInner, endInner.FindStartNode(out _));
    }

    [Fact]
    public void TestThatNestedRepeatReferencingOuterActiveIterationCalculatesHandledNodesProperly2()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();
        MathNode mathNode2 = new();

        startInner.Output.ConnectTo(mathNode.Y);
        startInner.CurrentIteration.ConnectTo(mathNode2.X);
        startOuter.CurrentIteration.ConnectTo(mathNode.Y);
        mathNode.Result.ConnectTo(mathNode2.Y);

        mathNode2.Result.ConnectTo(endInner.Input);

        startOuter.Output.ConnectTo(startInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(2, endInner.HandledNodes.Count);
        Assert.Contains(mathNode, endOuter.HandledNodes);
        Assert.Contains(mathNode2, endOuter.HandledNodes);

        Assert.Equal(4, endOuter.HandledNodes.Count);
        Assert.Contains(startInner, endOuter.HandledNodes);
        Assert.Contains(endInner, endOuter.HandledNodes);
        Assert.Contains(mathNode, endOuter.HandledNodes);
        Assert.Contains(mathNode2, endOuter.HandledNodes);
    }

    [Fact]
    public void TestThatNestedRepeatReferencingOuterActiveIterationCalculatesHandledNodesProperly()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        startOuter.Output.ConnectTo(startInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNode.X);
        startInner.Output.ConnectTo(mathNode.Y);

        mathNode.Result.ConnectTo(endInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Single(endInner.HandledNodes);
        Assert.Contains(mathNode, endOuter.HandledNodes);

        Assert.Equal(3, endOuter.HandledNodes.Count); // mathNode, startInner, endInner
        Assert.Contains(startInner, endOuter.HandledNodes);
        Assert.Contains(endInner, endOuter.HandledNodes);
    }

    [Fact]
    public void TestThatStartNodesAreCalculatedProperlyForNestedRepeats()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNodeToConnectToOuter = new();
        MathNode mathNodeToConnectToInner = new();
        MathNode mathNodeToCombineBoth = new();

        mathNodeToConnectToOuter.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };
        mathNodeToConnectToInner.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };

        startOuter.Output.ConnectTo(startInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNodeToConnectToOuter.Y);
        startInner.CurrentIteration.ConnectTo(mathNodeToConnectToInner.Y);
        mathNodeToConnectToOuter.Result.ConnectTo(mathNodeToCombineBoth.X);
        mathNodeToConnectToInner.Result.ConnectTo(mathNodeToCombineBoth.Y);
        mathNodeToCombineBoth.Result.ConnectTo(endInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(startOuter, endOuter.FindStartNode(out _));
        Assert.Equal(startInner, endInner.FindStartNode(out _));
    }

    [Fact]
    public void TestThatStartNodesAreCalculatedProperlyForUnrolledRepeatNodes()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNodeToConnectToOuter = new();
        MathNode mathNodeToConnectToInner = new();
        MathNode mathNodeToCombineBoth = new();

        mathNodeToConnectToOuter.X.NonOverridenValue = context => new Float1("") { ConstantValue = 2 };
        mathNodeToConnectToInner.X.NonOverridenValue = context => new Float1("") { ConstantValue = 2 };

        startOuter.Output.ConnectTo(startInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNodeToConnectToOuter.Y);
        startInner.CurrentIteration.ConnectTo(mathNodeToConnectToInner.Y);
        mathNodeToConnectToOuter.Result.ConnectTo(mathNodeToCombineBoth.X);
        mathNodeToConnectToInner.Result.ConnectTo(mathNodeToCombineBoth.Y);
        mathNodeToCombineBoth.Result.ConnectTo(endInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        var emptyContext = new RenderContext(null, new KeyFrameTime(), ChunkResolution.Full, VecI.Zero, VecI.Zero,
            ColorSpace.CreateSrgb(), SamplingOptions.Bilinear);

        var unrollQueue = endOuter.HandledNodes;

        var queue = startOuter.UnrollLoop(2, unrollQueue, emptyContext, emptyContext.ContextVirtualSession);

        var startNode = queue.OfType<RepeatNodeStart>().First();
        var endNode = queue.OfType<RepeatNodeEnd>().First();

        foreach (var node in queue)
        {
            emptyContext.SetActiveVirtualConnectionScope(emptyContext.ContextVirtualSession);
            node.Execute(emptyContext);
        }

        Assert.Equal(startNode.OtherNode, endNode.Id);
        Assert.Equal(endNode.OtherNode, startNode.Id);
        Assert.Equal(startNode.Id, endNode.OtherNode);
        Assert.Equal(endNode.Id, startNode.OtherNode);

        var queueInner = GraphUtils.CalculateExecutionQueue(endInner, false, true,
            property => property.Connection?.Node != startInner);
        var startNodeOuter = queueInner.OfType<RepeatNodeStart>().First();

        Assert.Equal(endOuter.Id, startNodeOuter.OtherNode);
    }

    [Fact]
    public void TestThatHandledNodesAreCalculatedProperlyForNestedRepeats()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNodeToConnectToOuter = new();
        MathNode mathNodeToConnectToInner = new();
        MathNode mathNodeToCombineBoth = new();

        mathNodeToConnectToOuter.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };
        mathNodeToConnectToInner.X.NonOverridenValue = context => new Float1("") { ConstantValue = 1 };

        startOuter.Output.ConnectTo(startInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNodeToConnectToOuter.Y);
        startInner.CurrentIteration.ConnectTo(mathNodeToConnectToInner.Y);
        mathNodeToConnectToOuter.Result.ConnectTo(mathNodeToCombineBoth.X);
        mathNodeToConnectToInner.Result.ConnectTo(mathNodeToCombineBoth.Y);
        mathNodeToCombineBoth.Result.ConnectTo(endInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        Assert.Equal(2, endInner.HandledNodes.Count);
        Assert.Contains(mathNodeToConnectToInner, endInner.HandledNodes);
        Assert.Contains(mathNodeToCombineBoth, endInner.HandledNodes);

        Assert.Equal(5, endOuter.HandledNodes.Count);
        Assert.Contains(startInner, endOuter.HandledNodes);
        Assert.Contains(endInner, endOuter.HandledNodes);
        Assert.Contains(mathNodeToConnectToOuter, endOuter.HandledNodes);
        Assert.Contains(mathNodeToConnectToInner, endOuter.HandledNodes);
        Assert.Contains(mathNodeToCombineBoth, endOuter.HandledNodes);
    }

    [Fact]
    public void TestThatNestedRepeatReferencingOuterActiveIterationCalculatesValueProperly()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode mathNode = new();

        startOuter.Output.ConnectTo(startInner.Input);
        startOuter.CurrentIteration.ConnectTo(mathNode.X);
        startInner.Output.ConnectTo(mathNode.Y);

        mathNode.Result.ConnectTo(endInner.Input);
        endInner.Output.ConnectTo(endOuter.Input);

        startOuter.Iterations.NonOverridenValue = 2;
        startInner.Iterations.NonOverridenValue = 10;

        RenderContext context = new(null, new KeyFrameTime(), ChunkResolution.Full, VecI.Zero, VecI.Zero,
            ColorSpace.CreateSrgb(), SamplingOptions.Bilinear);
        var executionQueue = GraphUtils.CalculateExecutionQueue(endOuter, true);

        Assert.Equal(startOuter.Id, executionQueue.ElementAt(0).Id);
        Assert.Equal(endOuter.Id, executionQueue.ElementAt(1).Id);

        foreach (var node in executionQueue)
        {
            node.Execute(context);
        }

        Assert.True(endOuter.Output.Value is Delegate);
        var func = (Delegate)endOuter.Output.Value!;
        var result = func.DynamicInvoke(ShaderFuncContext.NoContext);
        Assert.Equal(30, ((Float1)result!).ConstantValue);
    }

    [Fact]
    public void TestThatNestedCurrentIterationUnrollsProperly()
    {
        RepeatNodeStart startOuter = new();
        RepeatNodeEnd endOuter = new();
        RepeatNodeStart startInner = new();
        RepeatNodeEnd endInner = new();
        MathNode toConnectToOuter = new();
        MathNode toConnectToInner = new();
        CombineVecDNode toCombineBoth = new();

    }
}