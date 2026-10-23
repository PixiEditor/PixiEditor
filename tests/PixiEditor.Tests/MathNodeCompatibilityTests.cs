using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Tests;

public class MathNodeCompatibilityTests : FullPixiEditorTest
{
    [Theory]
    [InlineData("2.1.0.0", "PixiEditor.Math", true)]
    [InlineData("2.2.0.3", "PixiEditor.Math", true)]
    [InlineData("2.2.0.4", "PixiEditor.Math", true)]
    [InlineData("2.3.0.0", "PixiEditor.Math", true)]
    [InlineData(null, "PixiEditor.Math", false)]
    [InlineData("2.2.0.3", "PixiEditor.VectorMath", false)]
    public void UnmarkedMathNodesAreLegacyForAnyKnownVersion(string? version, string nodeName, bool expected)
    {
        Dictionary<string, object> data = new();
        CompatibilityUtility.UpgradeNodeAdditionalDataToCurrentVersion(
            data, version is null ? null : new Version(version), nodeName, new Dictionary<string, object>());

        Assert.Equal(expected, data.TryGetValue("legacyConstantStep", out var value) && value is true);
    }

    [Fact]
    public void ExplicitModernMarkerIsPreservedByRepeatedUpgrades()
    {
        Dictionary<string, object> data = new() { ["legacyConstantStep"] = false };
        for (int i = 0; i < 2; i++)
        {
            CompatibilityUtility.UpgradeNodeAdditionalDataToCurrentVersion(
                data, new Version(2, 2, 0, 3), "PixiEditor.Math", new Dictionary<string, object>());
        }

        Assert.Equal(false, data["legacyConstantStep"]);
    }

    [AvaloniaTheory]
    [InlineData("2.2.0.3", null, true)]
    [InlineData("2.2.0.4", null, true)]
    [InlineData("2.2.0.3", false, false)]
    [InlineData("2.2.0.4", true, true)]
    [InlineData("unknown", null, false)]
    public void LoadAndResavePreserveBothEvaluationPaths(string version, bool? marker, bool legacy)
    {
        using var original = BuildDocument();
        var serialized = original.ToSerializable();
        serialized.SerializerVersion = version;
        var serializedMath = Assert.Single(serialized.Graph.AllNodes, n => n.UniqueNodeName == "PixiEditor.Math");
        serializedMath.AdditionalData.Remove("legacyConstantStep");
        if (marker.HasValue)
        {
            serializedMath.AdditionalData["legacyConstantStep"] = marker.Value;
        }

        using var loaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(serialized)).ToDocument();
        AssertEvaluationPaths(loaded, legacy);

        var saved = loaded.ToSerializable();
        Assert.Equal(legacy, Assert.Single(saved.Graph.AllNodes, n => n.UniqueNodeName == "PixiEditor.Math")
            .AdditionalData["legacyConstantStep"]);
        using var reloaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(saved)).ToDocument();
        AssertEvaluationPaths(reloaded, legacy);
    }

    [AvaloniaFact]
    public void DuplicationImportAndDeleteUndoKeepLegacyEvaluation()
    {
        using var original = BuildDocument();
        var serialized = original.ToSerializable();
        serialized.SerializerVersion = "2.2.0.3";
        var serializedMath = Assert.Single(serialized.Graph.AllNodes, n => n.UniqueNodeName == "PixiEditor.Math");
        serializedMath.AdditionalData.Remove("legacyConstantStep");
        using var loaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(serialized)).ToDocument();
        var graph = loaded.AccessInternalReadOnlyDocument().NodeGraph;
        var math = Assert.Single(graph.AllNodes.OfType<MathNode>());
        Guid? duplicateId;
        using (var block = loaded.Operations.StartChangeBlock())
        {
            duplicateId = loaded.Operations.DuplicateNode(math.Id);
            block.ExecuteQueuedActions();
        }

        Assert.NotNull(duplicateId);
        var duplicate = Assert.Single(graph.AllNodes.OfType<MathNode>(), n => n.Id == duplicateId);
        Assert.Equal(1d, duplicate.Result.Value(new FuncContext()).GetConstant());
        Execute(loaded, () => loaded.Operations.DeleteNodes(new[] { duplicate.Id }));
        Assert.Single(graph.AllNodes.OfType<MathNode>());
        Execute(loaded, loaded.Operations.Undo);
        var restored = Assert.Single(graph.AllNodes.OfType<MathNode>(), n => n.Id == duplicateId);
        Assert.Equal(1d, restored.Result.Value(new FuncContext()).GetConstant());

        using var destination = BuildDocument();
        Guid? importedId;
        using (var block = destination.Operations.StartChangeBlock())
        {
            importedId = destination.Operations.ImportNode(math.Id, loaded);
            block.ExecuteQueuedActions();
        }

        Assert.NotNull(importedId);
        var imported = Assert.Single(destination.AccessInternalReadOnlyDocument().NodeGraph.AllNodes
            .OfType<MathNode>(), n => n.Id == importedId);
        Assert.Equal(1d, imported.Result.Value(new FuncContext()).GetConstant());
        var modern = Assert.Single(destination.AccessInternalReadOnlyDocument().NodeGraph.AllNodes
            .OfType<MathNode>(), n => n.Id != importedId);
        Assert.Equal(0d, modern.Result.Value(new FuncContext()).GetConstant());
    }

    [AvaloniaFact]
    public void LegacyMathKeepsItsStepSemanticsWhenChangingModes()
    {
        using var original = BuildDocument();
        var serialized = original.ToSerializable();
        serialized.SerializerVersion = "2.2.0.3";
        var serializedMath = Assert.Single(serialized.Graph.AllNodes, n => n.UniqueNodeName == "PixiEditor.Math");
        serializedMath.AdditionalData.Remove("legacyConstantStep");
        Assert.Single(serializedMath.InputPropertyValues, p => p.PropertyName == "Mode").Value = MathNodeMode.Add;
        using var loaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(serialized)).ToDocument();
        var math = Assert.Single(loaded.AccessInternalReadOnlyDocument().NodeGraph.AllNodes.OfType<MathNode>());
        Assert.Equal(0.75d, math.Result.Value(new FuncContext()).GetConstant());
        Execute(loaded, () => loaded.Operations.SetNodeInputPropertyValue(math.Id, "Mode", MathNodeMode.Step));
        AssertEvaluationPaths(loaded, true);
    }

    private static void Execute(DocumentViewModel document, Action action)
    {
        using var block = document.Operations.StartChangeBlock();
        action();
        block.ExecuteQueuedActions();
    }

    private static DocumentViewModel BuildDocument()
    {
        return DocumentViewModel.Build(b => b.WithSize(1, 1).WithGraph(g =>
        {
            g.WithNodeOfType<MathNode>(out int id).WithInputValues(new Dictionary<string, object>
            {
                [MathNode.ModePropertyName] = MathNodeMode.Step,
                [MathNode.XPropertyName] = 0.5,
                [MathNode.YPropertyName] = 0.25
            });
            g.WithNodeOfType<EllipseNode>(out _).WithConnections(new[]
            {
                new PropertyConnection
                {
                    InputPropertyName = "StrokeWidth",
                    OutputNodeId = id,
                    OutputPropertyName = "Result"
                }
            });
        }));
    }

    private static void AssertEvaluationPaths(DocumentViewModel document, bool legacy)
    {
        var graph = document.AccessInternalReadOnlyDocument().NodeGraph;
        var math = Assert.Single(graph.AllNodes.OfType<MathNode>());
        var ellipse = Assert.Single(graph.AllNodes.OfType<EllipseNode>());
        foreach (var (x, y, modern) in new[] { (0.5, 0.25, 0d), (0.5, 0.5, 1d), (0.5, 0.75, 1d) })
        {
            math.X.NonOverridenValue = _ => x;
            math.Y.NonOverridenValue = _ => y;
            Assert.Equal(legacy ? 1 - modern : modern, math.Result.Value(new FuncContext()).GetConstant());
            Assert.Equal(legacy ? 1 - modern : modern, ellipse.StrokeWidth.Value);

            ShaderBuilder builder = new(VecI.One);
            Float1 result = math.Result.Value(new FuncContext(null!, builder));
            builder.ReturnVar(builder.ConstructHalf4(result, result, result, (Float1)1.0), false);
            using var shader = builder.BuildShader();
            using var surface = Surface.ForProcessing(VecI.One, ColorSpace.CreateSrgb());
            using Paint paint = new() { Shader = shader };
            surface.DrawingSurface.Canvas.DrawRect(0, 0, 1, 1, paint);
            Assert.Equal(modern, surface.GetSrgbPixel(VecI.Zero).R / 255.0);
        }
    }
}
