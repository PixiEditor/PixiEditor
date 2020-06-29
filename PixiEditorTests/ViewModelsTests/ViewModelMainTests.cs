using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels;
using Xunit;

namespace PixiEditorTests.ViewModelsTests
{
    public class ViewModelMainTests
    {

        public ViewModelMainTests()
        {
            if (Application.Current == null)
            {
                var app = new App();
                app.InitializeComponent();
            }
        }

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

            viewModel.NewDocument(5,5);

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

            viewModel.MouseUpCommand.Execute(null);

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

    }
}
