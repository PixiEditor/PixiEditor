using Avalonia.Headless.XUnit;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.Models.IO;
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

        Assert.True(PixelCompare(renderedToCompare, toCompareTo));
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
        using Surface output = Surface.ForDisplay(document.SizeBindable);
        document.SceneRenderer.RenderScene(output.DrawingSurface, ChunkResolution.Half);

        Color expectedColor = Colors.Yellow;

        Assert.True(AllPixelsAreColor(output, expectedColor));
    }

    private static bool PixelCompare(Surface image, Surface compareTo)
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