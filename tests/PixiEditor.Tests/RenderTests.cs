using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Tests;

public class RenderTests : FullPixiEditorTest
{
    [AvaloniaFact]
    public void TestThatPixiFilesRenderTheSameResultAsSavedPng()
    {
        string[] files = Directory.GetFiles("TestFiles/RenderTests", "*.pixi");
        string[] results = Directory.GetFiles("TestFiles/RenderTests", "*.png");

        Assert.Equal(files.Length, results.Length);

        for (int i = 0; i < files.Length; i++)
        {
            string pixiFile = files[i];
            var document = Importer.ImportDocument(pixiFile);
            var pngFile = results.FirstOrDefault(x => x.EndsWith(Path.GetFileNameWithoutExtension(pixiFile) + ".png"));

            Assert.NotNull(pngFile);

            var result = document.TryRenderWholeImage(0);

            Assert.True(result is { IsT1: true, AsT1: not null }); // Check if rendering was successful

            using var image = result.AsT1;

            using var toCompareTo = Importer.ImportImage(results[i], document.SizeBindable);

            Assert.NotNull(toCompareTo);

            Assert.True(PixelCompare(image, toCompareTo));
        }
    }

    private static bool PixelCompare(Surface image, Surface compareTo)
    {
        if (image.Size != compareTo.Size)
        {
            return false;
        }

        Pixmap imagePixmap = image.PeekPixels();
        Pixmap compareToPixmap = compareTo.PeekPixels();

        if (imagePixmap == null || compareToPixmap == null)
        {
            return false;
        }

        for (int y = 0; y < image.Size.Y; y++)
        {
            for (int x = 0; x < image.Size.X; x++)
            {
                Color color1 = imagePixmap.GetPixelColor(x, y);
                Color color2 = compareToPixmap.GetPixelColor(x, y);

                if (color1 != color2)
                {
                    return false;
                }
            }
        }

        return true;
    }
}