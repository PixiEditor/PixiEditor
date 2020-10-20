using System.Collections.Generic;
using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class BitmapOperationsUtilityTests
    {
        [Fact]
        public void TestThatBitmapOperationsUtilityDeletesPixels()
        {
            var util = new BitmapOperationsUtility(new BitmapManager());

            var testLayer = new Layer("test layer", 10, 10);
            Coordinates[] cords = {new Coordinates(0, 0), new Coordinates(1, 1)};
            var pixels = BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Black);
            testLayer.SetPixels(pixels);

            util.DeletePixels(new[] {testLayer}, cords);

            Assert.Equal(0, testLayer.GetPixel(0, 0).A);
            Assert.Equal(0, testLayer.GetPixel(1, 1).A);
        }

        [StaFact]
        public void TestThatBitmapOperationsUtilityExecutesPenToolProperly()
        {
            var manager = new BitmapManager
            {
                ActiveDocument = new Document(10, 10),
                PrimaryColor = Colors.Black
            };
            manager.AddNewLayer("Test layer", 10, 10);

            var util = new BitmapOperationsUtility(manager);

            var mouseMove = new List<Coordinates>(new[] {new Coordinates(0, 0)});

            util.ExecuteTool(new Coordinates(0, 0), mouseMove, new MockedSinglePixelPen());
            Assert.Equal(manager.ActiveLayer.GetPixel(0, 0), Colors.Black);
        }
    }
}