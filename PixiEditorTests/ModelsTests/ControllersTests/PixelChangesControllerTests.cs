using System.Windows.Media;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class PixelChangesControllerTests
    {
        [Fact]
        public void TestThatPopChangesPopsChanges()
        {
            var controller = CreateBasicController();

            var changes = controller.PopChanges();
            Assert.NotEmpty(changes);
            Assert.Null(controller.PopChanges());
        }

        [Fact]
        public void TestThatAddChangesAddsAsNewChange()
        {
            var controller = CreateBasicController();
            Coordinates[] cords = {new Coordinates(5, 3), new Coordinates(7, 2)};

            controller.AddChanges(new LayerChange(
                    BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Black), 1),
                new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Transparent), 1));

            var changes = controller.PopChanges();
            Assert.Equal(2, changes.Length);
        }

        [Fact]
        public void TestThatAddChangesAddsToExistingChange()
        {
            Coordinates[] cords2 = {new Coordinates(2, 2), new Coordinates(5, 5)};
            var controller = CreateBasicController();

            controller.AddChanges(new LayerChange(
                    BitmapPixelChanges.FromSingleColoredArray(cords2, Colors.Black), 0),
                new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords2, Colors.Transparent), 0));

            var changes = controller.PopChanges();
            Assert.Single(changes);
            Assert.Equal(4, changes[0].Item1.PixelChanges.ChangedPixels.Count);
            Assert.Equal(4, changes[0].Item2.PixelChanges.ChangedPixels.Count);
        }

        private static PixelChangesController CreateBasicController()
        {
            Coordinates[] cords = {new Coordinates(0, 0), new Coordinates(1, 1)};
            var controller = new PixelChangesController();

            controller.AddChanges(new LayerChange(
                    BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Black), 0),
                new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords, Colors.Transparent), 0));
            return controller;
        }
    }
}