using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using PixiEditorTests.HelpersTests;
using SkiaSharp;
using System.IO;
using System.Windows.Input;
using Xunit;

namespace PixiEditorTests.ViewModelsTests
{
    [Collection("Application collection")]
    public class ViewModelMainTests
    {
        [StaFact]
        public void TestThatConstructorSetsUpControllersCorrectly()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            Assert.NotNull(viewModel.ChangesController);
            Assert.NotNull(viewModel.ShortcutController);
            Assert.NotEmpty(viewModel.ShortcutController.ShortcutGroups);
            Assert.NotNull(viewModel.BitmapManager);
            Assert.Equal(viewModel, ViewModelMain.Current);
        }

        [StaFact]
        public void TestThatSwapColorsCommandSwapsColors()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.ColorsSubViewModel.PrimaryColor = SKColors.Black;
            viewModel.ColorsSubViewModel.SecondaryColor = SKColors.White;

            viewModel.ColorsSubViewModel.SwapColorsCommand.Execute(null);

            Assert.Equal(SKColors.White, viewModel.ColorsSubViewModel.PrimaryColor);
            Assert.Equal(SKColors.Black, viewModel.ColorsSubViewModel.SecondaryColor);
        }

        [StaFact]
        public void TestThatNewDocumentCreatesNewDocumentWithBaseLayer()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.FileSubViewModel.NewDocument(5, 5);

            Assert.NotNull(viewModel.BitmapManager.ActiveDocument);
            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatMouseMoveCommandUpdatesCurrentCoordinates()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();
            viewModel.BitmapManager.ActiveDocument = new Document(10, 10);

            Assert.Equal(new Coordinates(0, 0), MousePositionConverter.CurrentCoordinates);

            viewModel.BitmapManager.ActiveDocument.MouseXOnCanvas = 5;
            viewModel.BitmapManager.ActiveDocument.MouseYOnCanvas = 5;

            viewModel.IoSubViewModel.MouseMoveCommand.Execute(null);

            Assert.Equal(new Coordinates(5, 5), MousePositionConverter.CurrentCoordinates);
        }

        [StaFact]
        public void TestThatSelectToolCommandSelectsNewTool()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            Assert.Equal(typeof(MoveViewportTool), viewModel.BitmapManager.SelectedTool.GetType());

            viewModel.ToolsSubViewModel.SelectToolCommand.Execute(new LineTool());

            Assert.Equal(typeof(LineTool), viewModel.BitmapManager.SelectedTool.GetType());
        }

        [StaFact]
        public void TestThatMouseUpCommandStopsRecordingMouseMovements()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.BitmapManager.MouseController.StartRecordingMouseMovementChanges(true);

            Assert.True(viewModel.BitmapManager.MouseController.IsRecordingChanges);

            viewModel.IoSubViewModel.MouseHook_OnMouseUp(default, default, MouseButton.Left);

            Assert.False(viewModel.BitmapManager.MouseController.IsRecordingChanges);
        }

        [StaFact]
        public void TestThatNewLayerCommandCreatesNewLayer()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            Assert.Empty(viewModel.BitmapManager.ActiveDocument.Layers);

            viewModel.LayersSubViewModel.NewLayerCommand.Execute(null);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatSaveDocumentCommandSavesFile()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();
            string fileName = "testFile.pixi";

            viewModel.BitmapManager.ActiveDocument = new Document(1, 1)
            {
                DocumentFilePath = fileName
            };

            viewModel.FileSubViewModel.SaveDocumentCommand.Execute(null);

            Assert.True(File.Exists(fileName));

            File.Delete(fileName);
        }

        [StaFact]
        public void TestThatAddSwatchAddsNonDuplicateSwatch()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();
            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            viewModel.ColorsSubViewModel.AddSwatch(SKColors.Lime);
            viewModel.ColorsSubViewModel.AddSwatch(SKColors.Lime);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Swatches);
            Assert.Equal(SKColors.Lime, viewModel.BitmapManager.ActiveDocument.Swatches[0]);
        }

        [StaTheory]
        [InlineData(5, 7)]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(16, 16)]
        [InlineData(50, 28)]
        [InlineData(120, 150)]
        public void TestThatSelectAllCommandSelectsWholeDocument(int docWidth, int docHeight)
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.BitmapManager.ActiveDocument = new Document(docWidth, docHeight);

            viewModel.BitmapManager.ActiveDocument.AddNewLayer("layer");

            viewModel.SelectionSubViewModel.SelectAllCommand.Execute(null);

            Assert.Equal(
                viewModel.BitmapManager.ActiveDocument.Width * viewModel.BitmapManager.ActiveDocument.Height,
                viewModel.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints.Count);
        }

        [StaFact]
        public void TestThatDocumentIsNotNullReturnsTrue()
        {
            ViewModelMain viewModel = ViewModelHelper.MockedViewModelMain();

            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            Assert.True(viewModel.DocumentIsNotNull(null));
        }
    }
}
