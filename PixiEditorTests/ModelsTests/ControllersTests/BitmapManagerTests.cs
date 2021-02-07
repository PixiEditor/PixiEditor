using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Undo;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class BitmapManagerTests
    {
        [Fact]
        public void TestThatBitmapManagerSetsCorrectTool()
        {
            BitmapManager bitmapManager = new BitmapManager();
            bitmapManager.SetActiveTool(new MockedSinglePixelPenTool());
            Assert.Equal(typeof(MockedSinglePixelPenTool), bitmapManager.SelectedTool.GetType());
        }

        [Fact]
        public void TestThatBitmapManagerAddsEmptyNewLayer()
        {
            string layerName = "TestLayer";
            BitmapManager bitmapManager = new BitmapManager
            {
                ActiveDocument = new Document(10, 10)
            };
            bitmapManager.ActiveDocument.AddNewLayer(layerName);
            Assert.Single(bitmapManager.ActiveDocument.Layers);
            Assert.Equal(layerName, bitmapManager.ActiveDocument.ActiveLayer.Name);
            Assert.Equal(0, bitmapManager.ActiveDocument.ActiveLayer.Width + bitmapManager.ActiveDocument.ActiveLayer.Height);
        }

        [Fact]
        public void TestThatBitmapManagerRemovesLayer()
        {
            BitmapManager bitmapManager = new BitmapManager
            {
                ActiveDocument = new Document(10, 10)
            };
            bitmapManager.ActiveDocument.AddNewLayer("_");
            bitmapManager.ActiveDocument.AddNewLayer("_1");
            Assert.Equal(2, bitmapManager.ActiveDocument.Layers.Count);
            bitmapManager.ActiveDocument.RemoveLayer(0);
            Assert.Single(bitmapManager.ActiveDocument.Layers);
        }

        [Fact]
        public void TestThatGeneratePreviewLayerGeneratesPreviewLayer()
        {
            BitmapManager bitmapManager = new BitmapManager
            {
                ActiveDocument = new Document(10, 10)
            };
            bitmapManager.ActiveDocument.GeneratePreviewLayer();
            Assert.NotNull(bitmapManager.ActiveDocument.PreviewLayer);
            Assert.Equal(0, bitmapManager.ActiveDocument.PreviewLayer.Width + bitmapManager.ActiveDocument.PreviewLayer.Height); // Size is zero
            Assert.Equal(0, bitmapManager.ActiveDocument.PreviewLayer.OffsetX + bitmapManager.ActiveDocument.PreviewLayer.OffsetY); // Offset is zero
            Assert.Equal(bitmapManager.ActiveDocument.Width, bitmapManager.ActiveDocument.PreviewLayer.MaxWidth);
            Assert.Equal(bitmapManager.ActiveDocument.Height, bitmapManager.ActiveDocument.PreviewLayer.MaxHeight);
        }

        [Fact]
        public void TestThatIsOperationToolWorks()
        {
            MockedSinglePixelPenTool singlePixelPen = new MockedSinglePixelPenTool();
            Assert.True(BitmapManager.IsOperationTool(singlePixelPen));
        }

        [StaFact]
        public void TestThatBitmapChangesExecuteToolExecutesPenTool()
        {
            BitmapManager bitmapManager = new BitmapManager
            {
                Documents = new System.Collections.ObjectModel.ObservableCollection<Document>()
                {
                    new Document(5, 5)
                }
            };

            bitmapManager.ActiveDocument = bitmapManager.Documents[0];

            bitmapManager.ActiveDocument.AddNewLayer("Layer");
            bitmapManager.SetActiveTool(new MockedSinglePixelPenTool());
            bitmapManager.PrimaryColor = Colors.Green;

            bitmapManager.MouseController.StartRecordingMouseMovementChanges(true);
            bitmapManager.MouseController.RecordMouseMovementChange(new Coordinates(1, 1));
            bitmapManager.MouseController.StopRecordingMouseMovementChanges();

            bitmapManager.ExecuteTool(new Coordinates(1, 1), true);

            Assert.Equal(Colors.Green, bitmapManager.ActiveLayer.GetPixelWithOffset(1, 1));
        }
    }
}