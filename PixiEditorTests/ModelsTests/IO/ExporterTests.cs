using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System.IO;
using System.Windows.Media.Imaging;
using Xunit;

namespace PixiEditorTests.ModelsTests.IO
{
    public class ExporterTests
    {
        //private const string FilePath = "test.file";

        //[Fact]
        //public void TestThatSaveAsPngSavesFile()
        //{
        //    Exporter.SaveAsPng(FilePath, 10, 10, BitmapFactory.New(10, 10));
        //    Assert.True(File.Exists(FilePath));

        //    File.Delete(FilePath);
        //}

        //[Fact]
        //public void TestThatSaveAsEditableFileSavesPixiFile()
        //{
        //    Document document = new Document(2, 2);

        //    string filePath = "testFile.pixi";

        //    document.Layers.Add(new Layer("layer1"));
        //    document.Layers[0].SetPixel(new Coordinates(1, 1), SKColors.White);

        //    document.Swatches.Add(SKColors.White);

        //    Exporter.SaveAsEditableFile(document, filePath);

        //    Assert.True(File.Exists(filePath));

        //    File.Delete(filePath);
        //}
    }
}
