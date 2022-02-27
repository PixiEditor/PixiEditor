using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using PixiEditorTests.ModelsTests.LayersTests;
using SkiaSharp;
using System;
using System.IO;
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

        public static Document GenerateTestDocument()
        {
            using Document testDocument = new Document(10, 10);
            using Surface testBitmap = new Surface(10, 10);
            using Surface testBitmap2 = new Surface(5, 8);
            testBitmap.SetSRGBPixel(0, 0, SKColors.Black);
            testBitmap2.SetSRGBPixel(4, 4, SKColors.Blue);
            Random random = new Random();
            testDocument.Layers = new WpfObservableRangeCollection<Layer>()
            {
                new Layer("Test layer" + random.Next(int.MinValue, int.MaxValue), testBitmap, testDocument.Width, testDocument.Height),
                new Layer("Test layer 2" + random.Next(int.MinValue, int.MaxValue), testBitmap2, testDocument.Width, testDocument.Height) { Offset = new System.Windows.Thickness(2, 3, 0, 0) }
            };
            return testDocument;
        }

        [Fact]
        public void TestThatConstructorGeneratesUndoLayersProperly()
        {
            using Document testDocument = GenerateTestDocument();

            using StorageBasedChange change = new StorageBasedChange(testDocument, testDocument.Layers, UndoStoreLocation);

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
            using Document document = GenerateTestDocument();

            using StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);

            foreach (var layer in change.StoredLayers)
            {
                Assert.True(File.Exists(layer.StoredPngLayerName));
                File.Delete(layer.StoredPngLayerName);
            }
        }

        [Fact]
        public void TestThatLoadLayersFromDeviceLoadsLayers()
        {
            using Document document = GenerateTestDocument();

            using StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);

            Layer[] layers = change.LoadLayersFromDevice();

            Assert.Equal(document.Layers.Count, layers.Length);
            for (int i = 0; i < document.Layers.Count; i++)
            {
                Layer expected = document.Layers[i];
                Layer actual = layers[i];
                LayersTestHelper.LayersAreEqual(expected, actual);
            }
        }

        [Fact]
        public void TestThatUndoInvokesLoadFromDeviceAndExecutesProcess()
        {
            using Document document = GenerateTestDocument();

            using StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);
            bool undoInvoked = false;

            Action<Layer[], UndoLayer[]> testUndoProcess = (layers, data) =>
            {
                undoInvoked = true;
                Assert.Equal(document.Layers.Count, layers.Length);
                Assert.Equal(document.Layers.Count, data.Length);
                foreach (var undoLayer in data)
                {
                    Assert.False(File.Exists(undoLayer.StoredPngLayerName));
                }
            };

            Action<object[]> testRedoProcess = parameters => { };

            Change undoChange = change.ToChange(testUndoProcess, testRedoProcess, null);
            using UndoManager manager = new UndoManager(this);

            manager.AddUndoChange(undoChange);
            manager.Undo();

            Assert.True(undoInvoked);
        }

        [Fact]
        public void TestThatRedoInvokesSaveToDeviceAndExecutesProcess()
        {
            using Document document = GenerateTestDocument();

            using StorageBasedChange change = new StorageBasedChange(document, document.Layers, UndoStoreLocation);
            bool redoInvoked = false;

            Action<Layer[], UndoLayer[]> testUndoProcess = (layers, data) => { };

            Action<object[]> testRedoProcess = parameters =>
            {
                redoInvoked = true;
                foreach (var undoLayer in change.StoredLayers)
                {
                    Assert.True(File.Exists(undoLayer.StoredPngLayerName));
                    Assert.NotNull(parameters);
                    Assert.Single(parameters);
                    Assert.IsType<int>(parameters[0]);
                    Assert.Equal(2, parameters[0]);
                }
            };

            Change undoChange = change.ToChange(testUndoProcess, testRedoProcess, new object[] { 2 });
            using UndoManager manager = new UndoManager(this);

            manager.AddUndoChange(undoChange);
            manager.Undo();
            manager.Redo();

            Assert.True(redoInvoked);
        }
    }
}
