using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using Xunit.Abstractions;
using Color = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Tests;

public class RenderTests : FullPixiEditorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RenderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [AvaloniaTheory]
    [InlineData("Fibi")]
    [InlineData("Pond")]
    [InlineData("SmlPxlCircShadWithMask")]
    [InlineData("SmallPixelArtCircleShadow")]
    [InlineData("SmlPxlCircShadWithMaskClipped")]
    [InlineData("SmlPxlCircShadWithMaskClippedInFolder")]
    [InlineData("VectorRectangleClippedToCircle")]
    [InlineData("VectorRectangleClippedToCircleShadowFilter")]
    [InlineData("VectorRectangleClippedToCircleMasked")]
    [InlineData("BlendingLinearSrgb")]
    [InlineData("BlendingSrgb")]
    [InlineData("VectorWithSepiaFilter")]
    [InlineData("VectorWithSepiaFilterSrgb")]
    [InlineData("VectorWithSepiaFilterChained")]
    [InlineData("Offset")]
    [InlineData("Scale")]
    [InlineData("Skew")]
    [InlineData("Rotation")]
    [InlineData("MatrixChain")]
    [InlineData("GpuOffset", "Offset")]
    [InlineData("GpuScale")]
    [InlineData("GpuSkew")]
    [InlineData("GpuMatrixChain")]
    [InlineData("FuncSwitch")]
    [InlineData("ContextlessConditional")]
    [InlineData("NestedElephants")]
    [InlineData("NestedColorSpace")]
    [InlineData("NestedFilter")]
    [InlineData("NestedColorSpaceOverlayAlphaFilter")]
    [InlineData("NestedVectorWithSepiaFilter")]
    [InlineData("ToolsetTextTests")]
    public void TestThatPixiFilesRenderTheSameResultAsSavedPng(string fileName, string? resultName = null)
    {
        if (!DrawingBackendApi.Current.IsHardwareAccelerated)
        {
            _testOutputHelper.WriteLine("Skipping the test because hardware acceleration is not enabled.");
            return;
        }

        string pixiFile = Path.Combine("TestFiles", "RenderTests", fileName + ".pixi");
        string pngFile = Path.Combine("TestFiles", "RenderTests", (resultName ?? fileName) + ".png");
        var document = Importer.ImportDocument(pixiFile);

        Assert.NotNull(pngFile);

        var result = document.TryRenderWholeImage(0);

        Assert.True(result is { IsT1: true, AsT1: not null }); // Check if rendering was successful

        using var image = result.AsT1;

        using var snapshot = image.DrawingSurface.Snapshot();
        using var encoded = snapshot.Encode();

        using var renderedToCompare = Surface.Load(encoded.AsSpan().ToArray());

        using var toCompareTo = Importer.ImportImage(pngFile, document.SizeBindable);

        Assert.NotNull(toCompareTo);

        bool matches = PixelCompare(renderedToCompare, toCompareTo);
        if (!matches)
        {
            var tmp = Path.Combine(Paths.TempFilesPath, "RenderTestFailures");
            Directory.CreateDirectory(tmp);
            string renderedPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(pixiFile) + "_rendered.png");
            string expectedPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(pixiFile) + "_expected.png");
            renderedToCompare.SaveTo(renderedPath);
            toCompareTo.SaveTo(expectedPath);

            using Surface diff = Surface.ForDisplay(document.SizeBindable);
            diff.DrawingSurface.Canvas.DrawSurface(toCompareTo.DrawingSurface, 0, 0);
            using var paint = new Paint
            {
                BlendMode = BlendMode.DstOut,
            };
            diff.DrawingSurface.Canvas.DrawSurface(renderedToCompare.DrawingSurface, 0, 0, paint);
            string diffPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(pixiFile) + "_diff.png");
            diff.SaveTo(diffPath);

            _testOutputHelper.WriteLine($"SVG rendering mismatch for file: {pixiFile}");
            _testOutputHelper.WriteLine($"Rendered image saved to: {renderedPath}");
        }

        Assert.True(matches, "Rendered pixi does not match the expected PNG for file: " + pixiFile);
    }

    [AvaloniaTheory]
    [InlineData("SingleLayer")]
    [InlineData("SingleLayerWithMask")]
    [InlineData("LayerWithMaskClipped")]
    [InlineData("LayerWithMaskClippedHighDpiPresent")]
    [InlineData("LayerWithMaskClippedInFolder")]
    [InlineData("LayerWithMaskClippedInFolderWithMask")]
    public void TestThatHalfResolutionScalesRenderCorrectly(string pixiName)
    {
        string pixiFile = Path.Combine("TestFiles", "ResolutionTests", pixiName + ".pixi");

        var document = Importer.ImportDocument(pixiFile);
        ViewportInfo info = new ViewportInfo(
            0,
            document.SizeBindable / 2f,
            document.SizeBindable,
            new ViewportData(),
            new PointerInfo(),
            new KeyboardInfo(),
            new EditorData(),
            null, "DEFAULT", SamplingOptions.Default, document.SizeBindable, ChunkResolution.Half,
            Guid.NewGuid(), false, false, () => { });
        using var output = document.SceneRenderer.RenderScene(info, new AffectedArea(), document.NodeGraph.GetHashCode());

        Color expectedColor = Colors.Yellow;

        using Surface surface = Surface.ForDisplay(document.SizeBindable);
        surface.DrawingSurface.Canvas.DrawSurface(output.DrawingSurface, 0, 0);

        Assert.True(AllPixelsAreColor(surface, expectedColor));
    }

    public static bool PixelCompare(Surface image, Surface compareTo)
    {
        if (image.Size != compareTo.Size)
        {
            return false;
        }

        using Surface compareSurface1 = new Surface(image.Size);
        using Surface compareSurface2 = new Surface(image.Size);

        compareSurface1.DrawingSurface.Canvas.DrawSurface(image.DrawingSurface, 0, 0);
        compareSurface2.DrawingSurface.Canvas.DrawSurface(compareTo.DrawingSurface, 0, 0);

        var imageData1 = compareSurface1.PeekPixels();
        var imageData2 = compareSurface2.PeekPixels();

        if (imageData1.Width != imageData2.Width || imageData1.Height != imageData2.Height)
        {
            return false;
        }

        for (int y = 0; y < imageData1.Height; y++)
        {
            for (int x = 0; x < imageData1.Width; x++)
            {
                var pixel1 = imageData1.GetPixelColor(x, y);
                var pixel2 = imageData2.GetPixelColor(x, y);

                if (pixel1 != pixel2)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool AllPixelsAreColor(Surface image, Color color)
    {
        var imageData = image.PeekPixels();

        for (int y = 0; y < imageData.Height; y++)
        {
            for (int x = 0; x < imageData.Width; x++)
            {
                var pixel = imageData.GetPixelColor(x, y);
                if (pixel != color)
                {
                    return false;
                }
            }
        }

        return true;
    }
}