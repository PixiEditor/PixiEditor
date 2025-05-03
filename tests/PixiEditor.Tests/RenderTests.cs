using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Tests;

public class RenderTests : FullPixiEditorTest
{
    [AvaloniaTheory]
    [InlineData("fibi")]
    [InlineData("Pond")]
    public void TestThatPixiFilesRenderTheSameResultAsSavedPng(string fileName)
    {
        string pixiFile = Path.Combine("TestFiles", "RenderTests", fileName + ".pixi");
        string pngFile = Path.Combine("TestFiles", "RenderTests", fileName + ".png");
        var document = Importer.ImportDocument(pixiFile);

        Assert.NotNull(pngFile);

        var result = document.TryRenderWholeImage(0);

        Assert.True(result is { IsT1: true, AsT1: not null }); // Check if rendering was successful

        using var image = result.AsT1;

        using var toCompareTo = Importer.ImportImage(pngFile, document.SizeBindable);

        Assert.NotNull(toCompareTo);

        Assert.True(PixelCompare(image, toCompareTo));
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
}