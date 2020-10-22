using System.IO;
using System.Windows.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels;
using Xunit;

namespace PixiEditorTests.ViewModelsTests
{
    [Collection("Application collection")]
    public class ViewModelMainTests
    {
        [StaFact]
        public void TestThatConstructorSetsUpControllersCorrectly()
        {
            ViewModelMain viewModel = new ViewModelMain();

            Assert.Equal(viewModel, UndoManager.MainRoot);
            Assert.NotNull(viewModel.ChangesController);
            Assert.NotNull(viewModel.ShortcutController);
            Assert.NotEmpty(viewModel.ShortcutController.Shortcuts);
            Assert.NotNull(viewModel.BitmapManager);
            Assert.Equal(viewModel, ViewModelMain.Current);
        }

        [StaFact]
        public void TestThatSwapColorsCommandSwapsColors()
        {
            ViewModelMain viewModel = new ViewModelMain();

            viewModel.PrimaryColor = Colors.Black;
            viewModel.SecondaryColor = Colors.White;

            viewModel.SwapColorsCommand.Execute(null);

            Assert.Equal(Colors.White, viewModel.PrimaryColor);
            Assert.Equal(Colors.Black, viewModel.SecondaryColor);
        }

        [StaFact]
        public void TestThatNewDocumentCreatesNewDocumentWithBaseLayer()
        {
            ViewModelMain viewModel = new ViewModelMain();

            viewModel.IoSubViewModel.NewDocument(5,5);

            Assert.NotNull(viewModel.BitmapManager.ActiveDocument);
            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatMouseMoveCommandUpdatesCurrentCoordinates()
        {
            ViewModelMain viewModel = new ViewModelMain();

            Assert.Equal(new Coordinates(0, 0), MousePositionConverter.CurrentCoordinates);

            viewModel.MouseXOnCanvas = 5;
            viewModel.MouseYOnCanvas = 5;

            viewModel.MouseMoveCommand.Execute(null);

            Assert.Equal(new Coordinates(5,5), MousePositionConverter.CurrentCoordinates);
        }

        [StaFact]
        public void TestThatSelectToolCommandSelectsNewTool()
        {
            ViewModelMain viewModel = new ViewModelMain();

            Assert.Equal(ToolType.Move,viewModel.BitmapManager.SelectedTool.ToolType);

            viewModel.SelectToolCommand.Execute(ToolType.Line);

            Assert.Equal(ToolType.Line, viewModel.BitmapManager.SelectedTool.ToolType);
        }

        [StaFact]
        public void TestThatMouseUpCommandStopsRecordingMouseMovements()
        {
            ViewModelMain viewModel = new ViewModelMain();

            viewModel.BitmapManager.MouseController.StartRecordingMouseMovementChanges(true);

            Assert.True(viewModel.BitmapManager.MouseController.IsRecordingChanges);

            viewModel.MouseHook_OnMouseUp(default, default, default);

            Assert.False(viewModel.BitmapManager.MouseController.IsRecordingChanges);
        }

        [StaFact]
        public void TestThatNewLayerCommandCreatesNewLayer()
        {
            ViewModelMain viewModel = new ViewModelMain();

            viewModel.BitmapManager.ActiveDocument = new Document(1,1);

            Assert.Empty(viewModel.BitmapManager.ActiveDocument.Layers);

            viewModel.NewLayerCommand.Execute(null);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatSaveDocumentCommandSavesFile()
        {
            ViewModelMain viewModel = new ViewModelMain();
            string fileName = "testFile.pixi";

            viewModel.BitmapManager.ActiveDocument = new Document(1,1);

            Exporter.SaveDocumentPath = fileName;

            viewModel.IoSubViewModel.SaveDocumentCommand.Execute(null);

            Assert.True(File.Exists(fileName));

            File.Delete(fileName);
        }

        [StaFact]
        public void TestThatAddSwatchAddsNonDuplicateSwatch()
        {
            ViewModelMain viewModel = new ViewModelMain();
            viewModel.BitmapManager.ActiveDocument = new Document(1,1);

            viewModel.AddSwatch(Colors.Green);
            viewModel.AddSwatch(Colors.Green);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Swatches);
            Assert.Equal(Colors.Green,viewModel.BitmapManager.ActiveDocument.Swatches[0]);
        }

        [StaTheory]
        [InlineData(5,7)]
        [InlineData(1,1)]
        [InlineData(1,2)]
        [InlineData(2,1)]
        [InlineData(16,16)]
        [InlineData(50,28)]
        [InlineData(120,150)]
        public void TestThatSelectAllCommandSelectsWholeDocument(int docWidth, int docHeight)
        {
            ViewModelMain viewModel = new ViewModelMain
            {
                BitmapManager = {ActiveDocument = new Document(docWidth, docHeight)}
            };
            viewModel.BitmapManager.AddNewLayer("layer");
            
            viewModel.SelectAllCommand.Execute(null);

            Assert.Equal(viewModel.BitmapManager.ActiveDocument.Width * viewModel.BitmapManager.ActiveDocument.Height,
                viewModel.ActiveSelection.SelectedPoints.Count);
        }

        [StaFact]
        public void TestThatDocumentIsNotNullReturnsTrue()
        {
            ViewModelMain viewModel = new ViewModelMain();

            viewModel.BitmapManager.ActiveDocument = new Document(1,1);

            Assert.True(viewModel.DocumentIsNotNull(null));
        }
    }
}
