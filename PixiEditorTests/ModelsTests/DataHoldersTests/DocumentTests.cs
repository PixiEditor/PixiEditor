using System;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels;
using PixiEditorTests.HelpersTests;
using PixiEditorTests.ModelsTests.ColorsTests;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    [Collection("Application collection")]
    public class DocumentTests
    {
        [Theory]
        [InlineData(10, 10, 20, 20)]
        [InlineData(1, 2, 5, 8)]
        [InlineData(20, 20, 10, 10)] // TODO Anchor
        public void TestResizeCanvasResizesProperly(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            Document document = new Document(oldWidth, oldHeight);

            document.ResizeCanvas(newWidth, newHeight, AnchorPoint.Top | AnchorPoint.Left);
            Assert.Equal(newHeight, document.Height);
            Assert.Equal(newWidth, document.Width);
        }

        [Theory]
        [InlineData(10, 10, 20, 20)]
        [InlineData(5, 8, 10, 16)]
        public void TestResizeWorks(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            Document document = new Document(oldWidth, oldHeight);

            document.Resize(newWidth, newHeight);

            Assert.Equal(newHeight, document.Height);
            Assert.Equal(newWidth, document.Width);
        }

        [Theory]
        [InlineData(10, 10, 0, 0)]
        [InlineData(50, 50, 10, 49)]
        public void TestThatClipCanvasWorksForSingleLayer(int initialWidth, int initialHeight, int additionalPixelX, int additionalPixelY)
        {
            Document document = new Document(initialWidth, initialHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = document
            };
            manager.ActiveDocument.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(
                new Coordinates(
                (int)Math.Ceiling(initialWidth / 2f),
                (int)Math.Ceiling(initialHeight / 2f)), ExtendedColorTests.black);

            manager.ActiveLayer.SetPixel(new Coordinates(additionalPixelX, additionalPixelY), ExtendedColorTests.black);

            document.ClipCanvas();

            Assert.Equal(manager.ActiveLayer.Width, document.Width);
            Assert.Equal(manager.ActiveLayer.Height, document.Height);
        }

        [Theory]
        [InlineData(10, 10, 0, 0)]
        [InlineData(50, 50, 15, 23)]
        [InlineData(3, 3, 1, 1)]
        [InlineData(1, 1, 0, 0)]
        public void TestThatClipCanvasWorksForMultipleLayers(int initialWidth, int initialHeight, int secondLayerPixelX, int secondLayerPixelY)
        {
            Document document = new Document(initialWidth, initialHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = document
            };
            manager.ActiveDocument.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(
                new Coordinates(
                (int)Math.Ceiling(initialWidth / 2f),
                (int)Math.Ceiling(initialHeight / 2f)), ExtendedColorTests.black); // Set pixel in center

            manager.ActiveDocument.AddNewLayer("test2");

            manager.ActiveLayer.SetPixel(new Coordinates(secondLayerPixelX, secondLayerPixelY), ExtendedColorTests.black);

            document.ClipCanvas();

            int totalWidth = Math.Abs(manager.ActiveDocument.Layers[1].OffsetX +
                manager.ActiveDocument.Layers[1].Width - (manager.ActiveDocument.Layers[0].OffsetX +
                                                          manager.ActiveDocument.Layers[0].Width)) + 1;

            int totalHeight = Math.Abs(manager.ActiveDocument.Layers[1].OffsetY +
                manager.ActiveDocument.Layers[1].Height - (manager.ActiveDocument.Layers[0].OffsetY +
                                                           manager.ActiveDocument.Layers[0].Height)) + 1;

            Assert.Equal(totalWidth, document.Width);
            Assert.Equal(totalHeight, document.Height);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(11, 11)]
        [InlineData(25, 17)]
        public void TestThatCenterContentCentersContentForSingleLayer(int docWidth, int docHeight)
        {
            Document doc = new Document(docWidth, docHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = doc
            };
            manager.ActiveDocument.AddNewLayer("test");

            manager.ActiveLayer.SetPixel(new Coordinates(0, 0), ExtendedColorTests.green);

            doc.CenterContent();

            Assert.Equal(Math.Floor(docWidth / 2f), manager.ActiveLayer.OffsetX);
            Assert.Equal(Math.Floor(docHeight / 2f), manager.ActiveLayer.OffsetY);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(11, 11)]
        [InlineData(25, 17)]
        public void TestThatCenterContentCentersContentForMultipleLayers(int docWidth, int docHeight)
        {
            Document doc = new Document(docWidth, docHeight);
            BitmapManager manager = new BitmapManager
            {
                ActiveDocument = doc
            };
            manager.ActiveDocument.AddNewLayer("test");
            manager.ActiveLayer.SetPixel(new Coordinates(0, 0), ExtendedColorTests.green);

            manager.ActiveDocument.AddNewLayer("test2");
            manager.ActiveLayer.SetPixel(new Coordinates(1, 1), ExtendedColorTests.green);

            foreach (var layer in manager.ActiveDocument.Layers)
            {
                layer.IsActive = true;
            }

            doc.CenterContent();

            int midWidth = (int)Math.Floor(docWidth / 2f);
            int midHeight = (int)Math.Floor(docHeight / 2f);

            Assert.Equal(midWidth - 1, manager.ActiveDocument.Layers[0].OffsetX);
            Assert.Equal(midHeight - 1, manager.ActiveDocument.Layers[0].OffsetY);

            Assert.Equal(midWidth, manager.ActiveDocument.Layers[1].OffsetX);
            Assert.Equal(midHeight, manager.ActiveDocument.Layers[1].OffsetY);
        }

        [Fact]
        public void TestThatSetNextActiveLayerSetsLayerBelow()
        {
            Document doc = new Document(10, 10);
            doc.Layers.Add(new PixiEditor.Models.Layers.Layer("Test"));
            doc.Layers.Add(new PixiEditor.Models.Layers.Layer("Test 2"));

            doc.SetMainActiveLayer(1);

            doc.SetNextLayerAsActive(1);

            Assert.False(doc.Layers[1].IsActive);
            Assert.True(doc.Layers[0].IsActive);
        }

        [Fact]
        public void TestThatAddNewLayerAddsUndoChange()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            Assert.Single(document.UndoManager.UndoStack);
        }

        [Fact]
        public void TestThatAddNewLayerUndoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            document.UndoManager.Undo();

            Assert.Single(document.Layers);
        }

        [Fact]
        public void TestThatAddNewLayerRedoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            document.UndoManager.Undo();
            document.UndoManager.Redo();

            Assert.Equal(2, document.Layers.Count);
        }

        [Fact]
        public void TestThatRemoveLayerUndoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            document.RemoveLayer(1);

            document.UndoManager.Undo();

            Assert.Equal(2, document.Layers.Count);
        }

        [Fact]
        public void TestThatRemoveLayerRedoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            document.RemoveLayer(1);

            document.UndoManager.Undo();
            document.UndoManager.Redo();

            Assert.Single(document.Layers);
        }

        [Theory]
        [InlineData(2, 0, 1)]
        [InlineData(2, 1, -1)]
        [InlineData(3, 1, 1)]
        [InlineData(3, 2, -2)]
        [InlineData(10, 9, -5)]
        public void TestThatMoveLayerInStructureWorks(int layersAmount, int index, int amount)
        {
            Document document = new Document(10, 10);
            for (int i = 0; i < layersAmount; i++)
            {
                document.AddNewLayer("Layer " + i);
            }

            Guid oldGuid = document.Layers[index].LayerGuid;
            Guid referenceGuid = document.Layers[index + amount].LayerGuid;
            document.MoveLayerInStructure(oldGuid, referenceGuid, amount > 0);

            Assert.Equal(oldGuid, document.Layers[index + amount].LayerGuid);
        }

        [Fact]
        public void TestThatMoveLayerInStructureUndoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            Guid oldGuid = document.Layers[0].LayerGuid;
            Guid referenceGuid = document.Layers[1].LayerGuid;

            document.MoveLayerInStructure(oldGuid, referenceGuid);

            document.UndoManager.Undo();

            Assert.Equal("Test2", document.Layers[1].Name);
            Assert.Equal("Test", document.Layers[0].Name);
        }

        [Fact]
        public void TestThatMoveLayerInStructureRedoProcessWorks()
        {
            Document document = new Document(10, 10);

            document.AddNewLayer("Test");
            document.AddNewLayer("Test2");

            Guid oldGuid = document.Layers[0].LayerGuid;
            Guid referenceGuid = document.Layers[1].LayerGuid;

            document.MoveLayerInStructure(oldGuid, referenceGuid, true);

            document.UndoManager.Undo();
            document.UndoManager.Redo();

            Assert.Equal("Test", document.Layers[1].Name);
            Assert.Equal("Test2", document.Layers[0].Name);
        }

        [StaFact]
        public void TestThatDocumentGetsAddedToRecentlyOpenedList()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            Document document = new Document(1, 1)
            {
                XamlAccesibleViewModel = viewModel
            };

            string testFilePath = @"C:\idk\somewhere\homework";

            document.DocumentFilePath = testFilePath;

            Assert.Contains(viewModel.FileSubViewModel.RecentlyOpened, x => x.FilePath == testFilePath);
        }

        [Fact]
        public void TestThatDupliacteLayerWorks()
        {
            const string layerName = "New Layer";

            Document document = new (10, 10);

            document.AddNewLayer(layerName);
            Layer duplicate = document.DuplicateLayer(0);

            Assert.Equal(document.Layers[1], duplicate);
            Assert.Equal(layerName + " (1)", duplicate.Name);
            Assert.True(duplicate.IsActive);
        }

        [Fact]
        public void TestThatCorrectLayerSuffixIsSet()
        {
            const string layerName = "New Layer";

            Document document = new (10, 10);

            document.AddNewLayer(layerName);
            document.AddNewLayer(layerName);
            document.AddNewLayer(layerName);

            Assert.Equal(layerName, document.Layers[0].Name);
            Assert.Equal(layerName + " (1)", document.Layers[1].Name);
            Assert.Equal(layerName + " (2)", document.Layers[2].Name);

            document.Layers.Add(new Layer(layerName + " (15)"));
            document.AddNewLayer(layerName);

            Assert.Equal(layerName + " (16)", document.Layers[4].Name);
        }
    }
}