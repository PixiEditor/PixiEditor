using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Rendering;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.Tests;

public class BlendingTests : PixiEditorTest
{
    [Theory]
    [InlineData(BlendMode.Normal, "#ffffff")]
    [InlineData(BlendMode.Erase, "#000000")]
    [InlineData(BlendMode.Darken, "#ff0000")]
    [InlineData(BlendMode.Multiply, "#ff0000")]
    [InlineData(BlendMode.ColorBurn, "#ff0000")]
    [InlineData(BlendMode.Lighten, "#ffffff")]
    [InlineData(BlendMode.Screen, "#ffffff")]
    [InlineData(BlendMode.ColorDodge, "#ff0000")]
    [InlineData(BlendMode.LinearDodge, "#ffffff")]
    [InlineData(BlendMode.Overlay, "#ff0000")]
    [InlineData(BlendMode.SoftLight, "#ff0000")]
    [InlineData(BlendMode.HardLight, "#ffffff")]
    [InlineData(BlendMode.Difference, "#00ffff")]
    [InlineData(BlendMode.Exclusion, "#00ffff")]
    [InlineData(BlendMode.Hue, "#949494")]
    [InlineData(BlendMode.Saturation, "#949494")]
    [InlineData(BlendMode.Luminosity, "#ffffff")]
    [InlineData(BlendMode.Color, "#949494")]
    public void TestThatBlendingWhiteOverRedBetweenTwoLayersInLinearSrgbWorksCorrectly(
        BlendMode blendMode,
        string? expectedColor)
    {
        NodeGraph graph = new NodeGraph();
        var firstImageLayer = new ImageLayerNode(new VecI(1, 1), ColorSpace.CreateSrgbLinear());

        using Paint redPaint = new Paint
        {
            Color = new Color(255, 0, 0, 255),
            BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src
        };

        var firstImg = firstImageLayer.GetLayerImageAtFrame(0);
        firstImg.EnqueueDrawPaint(redPaint);
        firstImg.CommitChanges();

        var secondImageLayer = new ImageLayerNode(new VecI(1, 1), ColorSpace.CreateSrgbLinear());
        var secondImg = secondImageLayer.GetLayerImageAtFrame(0);

        using var whitePaint = new Paint
        {
            Color = new Color(255, 255, 255, 255),
            BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.Src
        };

        secondImg.EnqueueDrawPaint(whitePaint);
        secondImg.CommitChanges();

        var outputNode = new OutputNode();
        graph.AddNode(firstImageLayer);
        graph.AddNode(secondImageLayer);
        graph.AddNode(outputNode);

        firstImageLayer.Output.ConnectTo(secondImageLayer.Background);
        secondImageLayer.Output.ConnectTo(outputNode.Input);

        secondImageLayer.BlendMode.NonOverridenValue = blendMode;

        Surface output = Surface.ForProcessing(VecI.One, ColorSpace.CreateSrgbLinear());
        graph.Execute(new RenderContext(output.DrawingSurface.Canvas, 0, ChunkResolution.Full, VecI.One, VecI.One, ColorSpace.CreateSrgbLinear(), SamplingOptions.Default, new NodeGraph(), 1));

        Color result = output.GetSrgbPixel(VecI.Zero);
        Assert.Equal(expectedColor, result.ToRgbHex());
    }
}