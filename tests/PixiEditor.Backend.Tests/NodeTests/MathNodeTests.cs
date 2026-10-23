using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;
using Xunit;

namespace PixiEditor.Backend.Tests.NodeTests;

public class MathNodeTests
{
    [Theory]
    [InlineData(0.5, 0.25, 0)]
    [InlineData(0.5, 0.5, 1)]
    [InlineData(0.5, 0.75, 1)]
    [InlineData(-0.5, -0.75, 0)]
    [InlineData(-0.5, -0.5, 1)]
    [InlineData(-0.5, -0.25, 1)]
    [InlineData(0, -0.0, 1)]
    [InlineData(double.PositiveInfinity, 0, 0)]
    [InlineData(0, double.PositiveInfinity, 1)]
    [InlineData(double.NegativeInfinity, double.NegativeInfinity, 1)]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(0, double.NaN, 0)]
    public void TestThatCpuStepWorks(double edge, double value, double expected)
    {
        MathNode node = new();
        node.X.NonOverridenValue = _ => edge;
        node.Y.NonOverridenValue = _ => value;
        node.Mode.NonOverridenValue = MathNodeMode.Step;

        foreach (bool clamp in new[] { false, true })
        {
            node.Clamp.NonOverridenValue = clamp;

            Assert.Equal(expected, node.Result.Value.Invoke(new FuncContext()).GetConstant());
        }
    }

    [Theory]
    [InlineData(0.5, 0.25, 1)]
    [InlineData(0.5, 0.5, 0)]
    [InlineData(0.5, 0.75, 0)]
    public void TestThatCpuGreaterThanWorks(double x, double y, double expected)
    {
        MathNode node = new();
        node.X.NonOverridenValue = _ => x;
        node.Y.NonOverridenValue = _ => y;
        node.Mode.NonOverridenValue = MathNodeMode.GreaterThan;

        Assert.Equal(expected, node.Result.Value.Invoke(new FuncContext()).GetConstant());
    }
    [Theory]
    [InlineData(0.5, 0.25, 1)]
    [InlineData(0.5, 0.5, 0)]
    [InlineData(0.5, 0.75, 0)]
    [InlineData(double.NaN, 0, 0)]
    [InlineData(0, double.NaN, 0)]
    [InlineData(double.PositiveInfinity, double.PositiveInfinity, 0)]
    public void TestThatLegacyStepSurvivesCopyAndSerialization(double x, double y, double expected)
    {
        using MathNode node = new();
        node.Mode.NonOverridenValue = MathNodeMode.Step;
        node.X.NonOverridenValue = _ => x;
        node.Y.NonOverridenValue = _ => y;
        node.DeserializeAdditionalData(
            null!, new Dictionary<string, object> { ["legacyConstantStep"] = true }, new());
        using MathNode clone = (MathNode)node.Clone();
        Dictionary<string, object> data = new();
        clone.SerializeAdditionalData(null!, data);
        using MathNode restored = new();
        restored.Mode.NonOverridenValue = MathNodeMode.Step;
        restored.X.NonOverridenValue = _ => x;
        restored.Y.NonOverridenValue = _ => y;
        restored.DeserializeAdditionalData(null!, data, new());

        foreach (var instance in new[] { node, clone, restored })
        {
            foreach (bool clamp in new[] { false, true })
            {
                instance.Clamp.NonOverridenValue = clamp;
                Assert.Equal(expected, instance.Result.Value(new FuncContext()).GetConstant());
            }
        }
    }

}
