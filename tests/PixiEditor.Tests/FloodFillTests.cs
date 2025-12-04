using Avalonia.Headless.XUnit;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Tests;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Backend.Tests;

public class FloodFillTests : FullPixiEditorTest
{
    [AvaloniaTheory]
    [InlineData(0, 64)]
    [InlineData(130, 64)]
    [InlineData(255, 64)]
    [InlineData(0, 512)]
    [InlineData(130, 512)]
    [InlineData(255, 512)]
    [InlineData(0, 2048)]
    [InlineData(130, 2048)]
    [InlineData(255, 2048)]
    public void TestThatFloodFillHelperFinishesLinearCs(byte alpha, int imgSize)
    {
        var doc = DocumentViewModel.Build(b => b.WithSize(imgSize, imgSize)
            .WithGraph(g =>
                g.WithImageLayerNode(
                        "layer", new VecI(imgSize), ColorSpace.CreateSrgbLinear(), out var id)
                    .WithOutputNode(id, "Output")));

        var color = Color.FromHsv(0f, 59.3f, 82.6f, alpha);

        var dict = FloodFillHelper.FloodFill([doc.NodeGraph.StructureTree.Members[0].Id],
            doc.AccessInternalReadOnlyDocument(),
            null, VecI.Zero, color, 0, 0, false, FloodFillMode.Replace);

        Assert.NotNull(dict);
        foreach (var kvp in dict)
        {
            Assert.NotNull(kvp.Value);
            var srgbPixel = kvp.Value.Surface.GetSrgbPixel(VecI.Zero);
            if (alpha == 0)
                Assert.Equal(Color.FromRgba(0, 0, 0, 0), srgbPixel);
            else
                Assert.Equal(color, srgbPixel);
        }
    }

    [AvaloniaTheory]
    [InlineData(0, 64)]
    [InlineData(130, 64)]
    [InlineData(255, 64)]
    [InlineData(0, 512)]
    [InlineData(130, 512)]
    [InlineData(255, 512)]
    [InlineData(0, 2048)]
    [InlineData(130, 2048)]
    [InlineData(255, 2048)]
    public void TestThatFloodFillHelperFinishesSrgbCs(byte alpha, int imgSize)
    {
        var doc = DocumentViewModel.Build(b => b.WithSize(imgSize, imgSize)
            .WithGraph(g =>
                g.WithImageLayerNode(
                    "layer", new VecI(imgSize), ColorSpace.CreateSrgb(), out var id).WithOutputNode(id, "Output")));

        FloodFillHelper.FloodFill([doc.NodeGraph.StructureTree.Members[0].Id], doc.AccessInternalReadOnlyDocument(),
            null, VecI.Zero,
            Color.FromHsv(0f, 59.3f, 82.6f, alpha), 0, 0, false, FloodFillMode.Replace);

        var color = Color.FromHsv(0f, 59.3f, 82.6f, alpha);

        var dict = FloodFillHelper.FloodFill([doc.NodeGraph.StructureTree.Members[0].Id],
            doc.AccessInternalReadOnlyDocument(),
            null, VecI.Zero, color, 0, 0, false, FloodFillMode.Replace);

        Assert.NotNull(dict);
        foreach (var kvp in dict)
        {
            Assert.NotNull(kvp.Value);
            var srgbPixel = kvp.Value.Surface.GetSrgbPixel(VecI.Zero);
            if (alpha == 0)
                Assert.Equal(Color.FromRgba(0, 0, 0, 0), srgbPixel);
            else
                Assert.Equal(color, srgbPixel);
        }
    }
}