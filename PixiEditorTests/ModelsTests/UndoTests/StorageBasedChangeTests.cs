using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditorTests.ModelsTests.LayersTests;
using Xunit;

namespace PixiEditorTests.ModelsTests.UndoTests
{
    public class StorageBasedChangeTests
    {
        private const string UndoStoreLocation = "undoStack";

        public StorageBasedChangeTests()
        {
            if (!Directory.Exists(UndoStoreLocation))
            {
                Directory.CreateDirectory(UndoStoreLocation);
            }
        }

        public Document GenerateTestDocument()
        {
            Document testDocument = new Document(10, 10);
            WriteableBitmap testBitmap = BitmapFactory.New(10, 10);
            WriteableBitmap testBitmap2 = BitmapFactory.New(5, 8);
            testBitmap.SetPixel(0, 0, Colors.Black);
            testBitmap2.SetPixel(4, 4, Colors.Beige);
            Random random = new Random();
            testDocument.Layers = new ObservableCollection<Layer>()
            {
                new Layer("Test layer" + random.Next(int.MinValue, int.MaxValue), testBitmap),
                new Layer("Test layer 2" + random.Next(int.MinValue, int.MaxValue), testBitmap2) { Offset = new System.Windows.Thickness(2, 3, 0, 0) }
            };
            return testDocument;
        }

        [Fact]
        public void TestThatConstructorGeneratesUndoLayersProperly()
        {
            Document testDocument = GenerateTestDocument();

            StorageBasedChange change = new StorageBasedChange(testDocument, testDocument.Layers, UndoStoreLocation);

            Assert.Equal(testDocument.Layers.Count, change.StoredLayers.Length);

            for (int i = 0; i < change.StoredLayers.Length; i++)
            {
                Layer testLayer = testDocument.Layers[i];
                UndoLayer layer = change.StoredLayers[i];

                Assert.Equal(testLayer.Name, layer.Name);
                Assert.Equal(testLayer.Width, layer.Width);
                Assert.Equal(testLayer.Height, layer.Height);
                Assert.Equal(testLayer.IsActive, layer.IsActive);
                Assert.Equal(testLayer.IsVisible, layer.IsVisible);
                Assert.Equal(testLayer.OffsetX, layer.OffsetX);
                Assert.Equal(testLayer.OffsetY, layer.OffsetY);
                Assert.Equal(testLayer.MaxWidth, layer.MaxWidth);
                Assert.Equal(testLayer.MaxHeight, layer.MaxHeight);
                Assert.Equal(testLayer.Opacity, layer.Opacity);
            }
        }

        [Fact]
        public void TestThatSaveLayersOnDeviceSavesLayers()
        {
            Document document = GenerateTestDocument();

            StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);
            change.SaveLayersOnDevice();

            foreach (var layer in change.StoredLayers)
            {
                Assert.True(File.Exists(layer.StoredPngLayerName));
                File.Delete(layer.StoredPngLayerName);
            }
        }

        [Fact]
        public void TestThatLoadLayersFromDeviceLoadsLayers()
        {
            Document document = GenerateTestDocument();

            StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);

            change.SaveLayersOnDevice();

            Layer[] layers = change.LoadLayersFromDevice();

            Assert.Equal(document.Layers.Count, layers.Length);
            for (int i = 0; i < document.Layers.Count; i++)
            {
                Layer expected = document.Layers[i];
                Layer actual = layers[i];
                LayersTestHelper.LayersAreEqual(expected, actual);
            }
        }
    }
}