using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class SerializableDocumentTests
    {

        [Fact]
        public void TestThatSerializableDocumentCreatesCorrectly()
        {
            Document document = GenerateSampleDocument();
            SerializableDocument doc = new SerializableDocument(document);

            var swatch = document.Swatches.First();
            Tuple<byte, byte, byte, byte> color = Tuple.Create(swatch.A, swatch.R, swatch.G, swatch.B);

            Assert.Equal(document.Width, doc.Width);
            Assert.Equal(document.Height, doc.Height);
            Assert.Equal(color, doc.Swatches.First());
            for (int i = 0; i < doc.Layers.Length; i++)
            {
                Assert.Equal(document.Layers[i].ConvertBitmapToBytes(), doc.Layers[i].BitmapBytes);
                Assert.Equal(document.Layers[i].OffsetX, doc.Layers[i].OffsetX);
                Assert.Equal(document.Layers[i].OffsetY, doc.Layers[i].OffsetY);
                Assert.Equal(document.Layers[i].Width, doc.Layers[i].Width);
                Assert.Equal(document.Layers[i].Height, doc.Layers[i].Height);
                Assert.Equal(document.Layers[i].MaxWidth, doc.Layers[i].MaxWidth);
                Assert.Equal(document.Layers[i].MaxHeight, doc.Layers[i].MaxHeight);
                Assert.Equal(document.Layers[i].IsVisible, doc.Layers[i].IsVisible);
                Assert.Equal(document.Layers[i].Opacity, doc.Layers[i].Opacity);
            }
        }

        [Fact]
        public void TestThatToDocumentConvertsCorrectly()
        {
            Document document = GenerateSampleDocument();
            SerializableDocument doc = new SerializableDocument(document);

            Document convertedDocument = doc.ToDocument();

            Assert.Equal(document.Height, convertedDocument.Height);
            Assert.Equal(document.Width, convertedDocument.Width);
            Assert.Equal(document.Swatches, convertedDocument.Swatches);
            Assert.Equal(document.Layers.Select(x=> x.LayerBitmap.ToByteArray()),
                convertedDocument.Layers.Select(x=> x.LayerBitmap.ToByteArray()));
        }

        [Fact]
        public void TestThatToLayersConvertsCorrectly()
        {
            Document document = GenerateSampleDocument();
            SerializableDocument doc = new SerializableDocument(document);

            var layers = doc.ToLayers();
            for (int i = 0; i < layers.Count; i++)
            {
                Assert.Equal(document.Layers[i].LayerBitmap.ToByteArray(), layers[i].ConvertBitmapToBytes());
                Assert.Equal(document.Layers[i].Height, layers[i].Height);
                Assert.Equal(document.Layers[i].Width, layers[i].Width);
                Assert.Equal(document.Layers[i].MaxHeight, layers[i].MaxHeight);
                Assert.Equal(document.Layers[i].MaxWidth, layers[i].MaxWidth);
                Assert.Equal(document.Layers[i].Offset, layers[i].Offset);
                Assert.Equal(document.Layers[i].Opacity, layers[i].Opacity);
                Assert.Equal(document.Layers[i].IsVisible, layers[i].IsVisible);
            }
        }

        private static Document GenerateSampleDocument()
        {
            Document document = new Document(10, 10);
            document.Layers.Add(new Layer("Test", 5, 8));
            document.Swatches.Add(Colors.Green);
            return document;
        }
    }
}
