using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using Xunit;

namespace PixiEditorTests.ViewModelsTests
{
    [Collection("Application collection")]
    public class ViewModelMainTests
    {
        public static IServiceProvider Services;

        public ViewModelMainTests()
        {
            Services = new ServiceCollection()
                .AddSingleton<IPreferences>(new Mocks.PreferenceSettingsMock())
                .BuildServiceProvider();
        }

        [StaFact]
        public void TestThatConstructorSetsUpControllersCorrectly()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            Assert.NotNull(viewModel.ChangesController);
            Assert.NotNull(viewModel.ShortcutController);
            Assert.NotEmpty(viewModel.ShortcutController.ShortcutGroups);
            Assert.NotNull(viewModel.BitmapManager);
            Assert.Equal(viewModel, ViewModelMain.Current);
        }

        [StaFact]
        public void TestThatSwapColorsCommandSwapsColors()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            viewModel.ColorsSubViewModel.PrimaryColor = Colors.Black;
            viewModel.ColorsSubViewModel.SecondaryColor = Colors.White;

            viewModel.ColorsSubViewModel.SwapColorsCommand.Execute(null);

            Assert.Equal(Colors.White, viewModel.ColorsSubViewModel.PrimaryColor);
            Assert.Equal(Colors.Black, viewModel.ColorsSubViewModel.SecondaryColor);
        }

        [StaFact]
        public void TestThatNewDocumentCreatesNewDocumentWithBaseLayer()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            viewModel.FileSubViewModel.NewDocument(5, 5);

            Assert.NotNull(viewModel.BitmapManager.ActiveDocument);
            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatMouseMoveCommandUpdatesCurrentCoordinates()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);
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
            ViewModelMain viewModel = new ViewModelMain(Services);

            Assert.Equal(typeof(MoveViewportTool), viewModel.BitmapManager.SelectedTool.GetType());

            viewModel.ToolsSubViewModel.SelectToolCommand.Execute(new LineTool());

            Assert.Equal(typeof(LineTool), viewModel.BitmapManager.SelectedTool.GetType());
        }

        [StaFact]
        public void TestThatMouseUpCommandStopsRecordingMouseMovements()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            viewModel.BitmapManager.MouseController.StartRecordingMouseMovementChanges(true);

            Assert.True(viewModel.BitmapManager.MouseController.IsRecordingChanges);

            viewModel.IoSubViewModel.MouseHook_OnMouseUp(default, default, MouseButton.Left);

            Assert.False(viewModel.BitmapManager.MouseController.IsRecordingChanges);
        }

        [StaFact]
        public void TestThatNewLayerCommandCreatesNewLayer()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            Assert.Empty(viewModel.BitmapManager.ActiveDocument.Layers);

            viewModel.LayersSubViewModel.NewLayerCommand.Execute(null);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Layers);
        }

        [StaFact]
        public void TestThatSaveDocumentCommandSavesFile()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);
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
            ViewModelMain viewModel = new ViewModelMain(Services);
            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            viewModel.ColorsSubViewModel.AddSwatch(Colors.Green);
            viewModel.ColorsSubViewModel.AddSwatch(Colors.Green);

            Assert.Single(viewModel.BitmapManager.ActiveDocument.Swatches);
            Assert.Equal(Colors.Green, viewModel.BitmapManager.ActiveDocument.Swatches[0]);
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
            ViewModelMain viewModel = new ViewModelMain(Services)
            {
                BitmapManager = { ActiveDocument = new Document(docWidth, docHeight) }
            };
            viewModel.BitmapManager.ActiveDocument.AddNewLayer("layer");

            viewModel.SelectionSubViewModel.SelectAllCommand.Execute(null);

            Assert.Equal(
                viewModel.BitmapManager.ActiveDocument.Width * viewModel.BitmapManager.ActiveDocument.Height,
                viewModel.BitmapManager.ActiveDocument.ActiveSelection.SelectedPoints.Count);
        }

        [StaFact]
        public void TestThatDocumentIsNotNullReturnsTrue()
        {
            ViewModelMain viewModel = new ViewModelMain(Services);

            viewModel.BitmapManager.ActiveDocument = new Document(1, 1);

            Assert.True(viewModel.DocumentIsNotNull(null));
        }
    }
}