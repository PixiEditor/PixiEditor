using Avalonia.Headless.XUnit;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels;
using Xunit.Abstractions;

namespace PixiEditor.Tests;

public class SvgRenderTests : FullPixiEditorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SvgRenderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [AvaloniaFact]
    public void TestSvgRenderingWithPngComparison()
    {
        // Load svg from /TestFiles/SvgRenderTests/*.svg
        // Load respective png from /TestFiles/SvgRenderTests/*.png
        // Render svg using PixiEditor's rendering engine

        string[] svgFiles = Directory.GetFiles(Path.Combine("TestFiles", "SvgRenderTests"), "*.svg");
        string[] pngFiles = Directory.GetFiles(Path.Combine("TestFiles", "SvgRenderTests"), "*.png");

        foreach (var svgFile in svgFiles)
        {
            string pngFile = Path.ChangeExtension(svgFile, ".png");

            if (!File.Exists(pngFile))
            {
                Assert.Fail($"No corresponding PNG file found for SVG file: {svgFile}");
            }

            var document = ViewModelMain.Current.FileSubViewModel.OpenFromPath(svgFile);

            Assert.NotNull(pngFile);

            var result = document.TryRenderWholeImage(0);

            Assert.True(result is { IsT1: true, AsT1: not null }); // Check if rendering was successful

            using var image = result.AsT1;

            using var snapshot = image.DrawingSurface.Snapshot();
            using var encoded = snapshot.Encode();

            using var renderedToCompare = Surface.Load(encoded.AsSpan().ToArray());

            using var toCompareTo = Importer.ImportImage(pngFile, document.SizeBindable);

            Assert.NotNull(toCompareTo);

            VecI size = document.SizeBindable;
            Assert.Equal(size, toCompareTo.Size);

            bool matches = RenderTests.PixelCompare(renderedToCompare, toCompareTo);

            if (!matches)
            {
                var tmp = Path.Combine(Paths.TempFilesPath, "SvgRenderTestFailures");
                Directory.CreateDirectory(tmp);
                string renderedPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(svgFile) + "_rendered.png");
                string expectedPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(svgFile) + "_expected.png");
                renderedToCompare.SaveTo(renderedPath);
                toCompareTo.SaveTo(expectedPath);

                using Surface diff = Surface.ForDisplay(size);
                diff.DrawingSurface.Canvas.DrawSurface(toCompareTo.DrawingSurface, 0, 0);
                using var paint = new Paint
                {
                    BlendMode = BlendMode.DstOut,
                };
                diff.DrawingSurface.Canvas.DrawSurface(renderedToCompare.DrawingSurface, 0, 0, paint);
                string diffPath = Path.Combine(tmp, Path.GetFileNameWithoutExtension(svgFile) + "_diff.png");
                diff.SaveTo(diffPath);

                _testOutputHelper.WriteLine($"SVG rendering mismatch for file: {svgFile}");
                _testOutputHelper.WriteLine($"Rendered image saved to: {renderedPath}");
            }

            Assert.True(matches, "Rendered SVG does not match the expected PNG for file: " + svgFile);
        }
    }
}