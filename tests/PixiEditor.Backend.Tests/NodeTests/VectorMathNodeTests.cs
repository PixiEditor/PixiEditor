using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Tests;

namespace PixiEditor.Backend.Tests.NodeTests;

public class VectorMathNodeTests : PixiEditorTest
{
    [Theory]
    [InlineData(1, 2, 3, 4, 4, 6)]
    [InlineData(-1, -2, -3, -4, -4, -6)]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(1.5, 2.5, 3.5, 4.5, 5, 7)]
    public void TestThatCpuAddWorks(double x1, double y1, double x2, double y2, double expectedX1, double expectedY1)
    {
        VectorMathNode node = new VectorMathNode();
        node.X.NonOverridenValue = (context => new VecD(x1, y1));
        node.Y.NonOverridenValue = (context => new VecD(x2, y2));
        node.Mode.NonOverridenValue = VectorMathMode.Add;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(expectedX1, expectedY1), node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Theory]
    [InlineData(1, 2, 3, 4, -2, -2)]
    [InlineData(-1, -2, -3, -4, 2, 2)]
    [InlineData(0, 0, 0, 0, 0, 0)]
    [InlineData(1.5, 2.5, 3.5, 4.5, -2, -2)]
    public void TestThatCpuSubtractWorks(double x1, double y1, double x2, double y2, double expectedX1,
        double expectedY1)
    {
        VectorMathNode node = new VectorMathNode();
        node.X.NonOverridenValue = (context => new VecD(x1, y1));
        node.Y.NonOverridenValue = (context => new VecD(x2, y2));
        node.Mode.NonOverridenValue = VectorMathMode.Subtract;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(expectedX1, expectedY1), node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Theory]
    [InlineData(2, 3, 4, 5, 8, 15)]
    [InlineData(-2, 3, 4, -5, -8, -15)]
    public void TestThatCpuMultiplyWorks(double x1, double y1, double x2, double y2, double expectedX, double expectedY)
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(x1, y1);
        node.Y.NonOverridenValue = _ => new VecD(x2, y2);
        node.Mode.NonOverridenValue = VectorMathMode.Multiply;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(expectedX, expectedY),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Theory]
    [InlineData(8, 15, 2, 5, 4, 3)]
    [InlineData(10, 20, 0, 4, 0, 5)]
    public void TestThatCpuDivideWorks(double x1, double y1, double x2, double y2, double expectedX, double expectedY)
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(x1, y1);
        node.Y.NonOverridenValue = _ => new VecD(x2, y2);
        node.Mode.NonOverridenValue = VectorMathMode.Divide;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(expectedX, expectedY),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuNegateWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(3, -4);
        node.Mode.NonOverridenValue = VectorMathMode.Negate;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(-3, 4),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuAbsoluteWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(-3, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Absolute;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(3, 4),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuFloorWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(1.9, -1.2);
        node.Mode.NonOverridenValue = VectorMathMode.Floor;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(1, -2),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuCeilWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(1.2, -1.8);
        node.Mode.NonOverridenValue = VectorMathMode.Ceil;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(2, -1),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuRoundWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(1.4, 1.6);
        node.Mode.NonOverridenValue = VectorMathMode.Round;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(1, 2),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuFractionWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(1.75, -1.25);
        node.Mode.NonOverridenValue = VectorMathMode.Fraction;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(0.75, 0.75),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Theory]
    [InlineData(7, 12, 5, 10, 2, 2)]
    [InlineData(-1, -1, 5, 5, 4, 4)]
    [InlineData(-6, -11, 5, 10, 4, 9)]
    [InlineData(6, 11, -5, -10, -4, -9)]
    [InlineData(-6, 11, 5, 10, 4, 1)]
    [InlineData(6, -11, 5, 10, 1, 9)]
    public void TestThatCpuModuloWorks(
        double x1,
        double y1,
        double x2,
        double y2,
        double expectedX,
        double expectedY)
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(x1, y1);
        node.Y.NonOverridenValue = _ => new VecD(x2, y2);
        node.Mode.NonOverridenValue = VectorMathMode.Modulo;

        node.Execute(CreateEmptyContext());

        Assert.Equal(
            new VecD(expectedX, expectedY),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuModuloByZeroReturnsNaN()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(5, 10);
        node.Y.NonOverridenValue = _ => new VecD(0, 0);
        node.Mode.NonOverridenValue = VectorMathMode.Modulo;

        node.Execute(CreateEmptyContext());

        VecD result = (VecD)node.Result.Value
            .Invoke(FuncContext.NoContext)
            .GetConstant();

        Assert.True(double.IsNaN(result.X));
        Assert.True(double.IsNaN(result.Y));
    }

    [Fact]
    public void TestThatCpuMinWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(2, 8);
        node.Y.NonOverridenValue = _ => new VecD(5, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Min;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(2, 4),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuMaxWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(2, 8);
        node.Y.NonOverridenValue = _ => new VecD(5, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Max;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(5, 8),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuMultiplyAddWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(2, 3);
        node.Y.NonOverridenValue = _ => new VecD(4, 5);
        node.Z.NonOverridenValue = _ => new VecD(1, 2);
        node.Mode.NonOverridenValue = VectorMathMode.MultiplyAdd;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(9, 17),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuScaleWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(2, 3);
        node.S.NonOverridenValue = _ => 4d;
        node.Mode.NonOverridenValue = VectorMathMode.Scale;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(8, 12),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuNormalizeWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(3, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Normalize;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(0.6, 0.8),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuSignWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(-5, 0);
        node.Mode.NonOverridenValue = VectorMathMode.Sign;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(-1, 0),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuWrapWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(12, -2);
        node.Y.NonOverridenValue = _ => new VecD(0, 0);
        node.Z.NonOverridenValue = _ => new VecD(10, 10);
        node.Mode.NonOverridenValue = VectorMathMode.Wrap;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(2, 8),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuSnapWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(13, 27);
        node.Y.NonOverridenValue = _ => new VecD(5, 10);
        node.Mode.NonOverridenValue = VectorMathMode.Snap;

        node.Execute(CreateEmptyContext());

        Assert.Equal(new VecD(10, 20),
            node.Result.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuDotWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(1, 2);
        node.Y.NonOverridenValue = _ => new VecD(3, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Dot;

        node.Execute(CreateEmptyContext());

        Assert.Equal(11d,
            node.ResultFloat1.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuDistanceWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(0, 0);
        node.Y.NonOverridenValue = _ => new VecD(3, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Distance;

        node.Execute(CreateEmptyContext());

        Assert.Equal(5d,
            node.ResultFloat1.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    [Fact]
    public void TestThatCpuLengthWorks()
    {
        VectorMathNode node = new();
        node.X.NonOverridenValue = _ => new VecD(3, 4);
        node.Mode.NonOverridenValue = VectorMathMode.Length;

        node.Execute(CreateEmptyContext());

        Assert.Equal(5d,
            node.ResultFloat1.Value.Invoke(FuncContext.NoContext).GetConstant());
    }

    private static RenderContext CreateEmptyContext()
    {
        return new RenderContext(null, 1, ChunkResolution.Full, VecI.Zero, VecI.Zero, ColorSpace.CreateSrgb(),
            new SamplingOptions(), null);
    }
}