using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.CompatibilityUpgrades;

namespace PixiEditor.Tests;

public class MathNodeCompatibilityTests : FullPixiEditorTest
{
    [AvaloniaTheory]
    [InlineData("2.1.0.0", true)]
    [InlineData("2.2.0.3", true)]
    [InlineData("2.2.0.4", true)]
    [InlineData("2.2.0.5", false)]
    [InlineData("2.3.0.0", false)]
    [InlineData("unknown", false)]
    public void CpuStepMigrationIsVersionedAndSurvivesSaveReload(string version, bool legacy)
    {
        using var original = BuildDocument();
        using var loaded = LoadAsVersion(original, version);
        AssertCpu(loaded, legacy);

        var saved = loaded.ToSerializable();
        Assert.True(new Version(saved.SerializerVersion) >= new Version(2, 2, 0, 5));
        Assert.DoesNotContain("legacyConstantStep",
            Assert.Single(saved.Graph.AllNodes, n => n.UniqueNodeName == "PixiEditor.Math").AdditionalData.Keys);
        using var reloaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(saved)).ToDocument();
        AssertCpu(reloaded, legacy);

        // Conversion is also idempotent if requested again for an old document.
        if (legacy)
        {
            new StepToGreaterThanConverter(reloaded).Upgrade();
            AssertCpu(reloaded, true);
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShaderReachableStepRetainsItsModeAndShaderOutput(bool alsoCpuConsumer)
    {
        using var original = BuildDocument(shader: true, cpu: alsoCpuConsumer);
        using var loaded = LoadAsVersion(original, "2.2.0.4");
        var graph = loaded.AccessInternalReadOnlyDocument().NodeGraph;
        var math = Assert.Single(graph.AllNodes.OfType<MathNode>());
        Assert.Equal(MathNodeMode.Step, math.Mode.Value);

        foreach (var (x, y, expected) in new[] { (0.5, 0.25, 0d), (0.5, 0.5, 1d), (0.5, 0.75, 1d) })
        {
            math.X.NonOverridenValue = _ => x;
            math.Y.NonOverridenValue = _ => y;
            ShaderBuilder builder = new(VecI.One);
            Float1 result = math.Result.Value(new FuncContext(null!, builder));
            builder.ReturnVar(builder.ConstructHalf4(result, result, result, (Float1)1.0), false);
            using var shader = builder.BuildShader();
            using var surface = Surface.ForProcessing(VecI.One, ColorSpace.CreateSrgb());
            using Paint paint = new() { Shader = shader };
            surface.DrawingSurface.Canvas.DrawRect(0, 0, 1, 1, paint);
            Assert.Equal(expected, surface.GetSrgbPixel(VecI.Zero).R / 255.0);
            if (alsoCpuConsumer)
            {
                // The existing context classifier gives shader reachability priority.
                Assert.Equal(expected, Assert.Single(graph.AllNodes.OfType<EllipseNode>()).StrokeWidth.Value);
            }
        }
    }

    [AvaloniaTheory]
    [InlineData(MathNodeMode.Add)]
    [InlineData(MathNodeMode.GreaterThan)]
    public void OtherModesAreNotConverted(MathNodeMode mode)
    {
        using var original = BuildDocument(mode: mode);
        using var loaded = LoadAsVersion(original, "2.2.0.4");
        Assert.Equal(mode, Assert.Single(loaded.AccessInternalReadOnlyDocument().NodeGraph.AllNodes
            .OfType<MathNode>()).Mode.Value);
    }

    [AvaloniaFact]
    public void NewlyCreatedStepIsNotConverted()
    {
        using var document = BuildDocument();
        AssertCpu(document, false);
        using var reloaded = PixiParser.V5.Deserialize(PixiParser.V5.Serialize(document.ToSerializable())).ToDocument();
        AssertCpu(reloaded, false);
    }

    [AvaloniaFact]
    public void ConvertedModeSurvivesDuplicationImportAndDeleteUndo()
    {
        using var original = BuildDocument();
        using var loaded = LoadAsVersion(original, "2.2.0.4");
        var graph = loaded.AccessInternalReadOnlyDocument().NodeGraph;
        var math = Assert.Single(graph.AllNodes.OfType<MathNode>());
        Guid? duplicateId;
        using (var block = loaded.Operations.StartChangeBlock())
        {
            duplicateId = loaded.Operations.DuplicateNode(math.Id);
            block.ExecuteQueuedActions();
        }

        var duplicate = Assert.Single(graph.AllNodes.OfType<MathNode>(), n => n.Id == duplicateId);
        Assert.Equal(MathNodeMode.GreaterThan, duplicate.Mode.Value);
        Execute(loaded, () => loaded.Operations.DeleteNodes(new[] { duplicate.Id }));
        Execute(loaded, loaded.Operations.Undo);
        Assert.Equal(MathNodeMode.GreaterThan,
            Assert.Single(graph.AllNodes.OfType<MathNode>(), n => n.Id == duplicateId).Mode.Value);

        using var destination = BuildDocument();
        Guid? importedId;
        using (var block = destination.Operations.StartChangeBlock())
        {
            importedId = destination.Operations.ImportNode(math.Id, loaded);
            block.ExecuteQueuedActions();
        }

        Assert.Equal(MathNodeMode.GreaterThan, Assert.Single(destination.AccessInternalReadOnlyDocument()
            .NodeGraph.AllNodes.OfType<MathNode>(), n => n.Id == importedId).Mode.Value);
    }

    private static DocumentViewModel LoadAsVersion(DocumentViewModel document, string version)
    {
        var serialized = document.ToSerializable();
        serialized.SerializerVersion = version;
        return PixiParser.V5.Deserialize(PixiParser.V5.Serialize(serialized)).ToDocument();
    }

    private static void Execute(DocumentViewModel document, Action action)
    {
        using var block = document.Operations.StartChangeBlock();
        action();
        block.ExecuteQueuedActions();
    }

    private static DocumentViewModel BuildDocument(bool shader = false, bool cpu = true,
        MathNodeMode mode = MathNodeMode.Step)
    {
        return DocumentViewModel.Build(b => b.WithSize(1, 1).WithGraph(g =>
        {
            g.WithNodeOfType<MathNode>(out int id).WithInputValues(new Dictionary<string, object>
            {
                [MathNode.ModePropertyName] = mode,
                [MathNode.XPropertyName] = 0.5,
                [MathNode.YPropertyName] = 0.25
            });
            if (cpu)
            {
                g.WithNodeOfType<EllipseNode>(out _).WithConnections(new[]
                {
                    new PropertyConnection { InputPropertyName = "StrokeWidth", OutputNodeId = id, OutputPropertyName = "Result" }
                });
            }
            if (shader)
            {
                g.WithNodeOfType<CombineColorNode>(out int color).WithConnections(new[]
                {
                    new PropertyConnection { InputPropertyName = "R", OutputNodeId = id, OutputPropertyName = "Result" }
                });
                g.WithNodeOfType<ModifyImageRightNode>(out _).WithConnections(new[]
                {
                    new PropertyConnection { InputPropertyName = "Color", OutputNodeId = color, OutputPropertyName = "Color" }
                });
            }
        }));
    }

    private static void AssertCpu(DocumentViewModel document, bool legacy)
    {
        var graph = document.AccessInternalReadOnlyDocument().NodeGraph;
        var math = Assert.Single(graph.AllNodes.OfType<MathNode>());
        var ellipse = Assert.Single(graph.AllNodes.OfType<EllipseNode>());
        Assert.Equal(legacy ? MathNodeMode.GreaterThan : MathNodeMode.Step, math.Mode.Value);
        foreach (var (x, y, modern) in new[] { (0.5, 0.25, 0d), (0.5, 0.5, 1d), (0.5, 0.75, 1d) })
        {
            math.X.NonOverridenValue = _ => x;
            math.Y.NonOverridenValue = _ => y;
            Assert.Equal(legacy ? 1 - modern : modern, math.Result.Value(new FuncContext()).GetConstant());
            Assert.Equal(legacy ? 1 - modern : modern, ellipse.StrokeWidth.Value);
        }
    }
}
