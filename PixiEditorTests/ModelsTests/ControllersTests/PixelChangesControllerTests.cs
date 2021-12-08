using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using Xunit;

namespace PixiEditorTests.ModelsTests.ControllersTests
{
    public class PixelChangesControllerTests
    {
        [Fact]
        public void TestThatPopChangesPopsChanges()
        {
            //PixelChangesController controller = CreateBasicController().Item2;

            //System.Tuple<LayerChange, LayerChange>[] changes = controller.PopChanges();
            //Assert.NotEmpty(changes);
            //Assert.Null(controller.PopChanges());
        }

        [Fact]
        public void TestThatAddChangesAddsAsNewChange()
        {
            //var data = CreateBasicController();
            //PixelChangesController controller = data.Item2;
            //Coordinates[] cords = { new Coordinates(5, 3), new Coordinates(7, 2) };
            //Guid guid = Guid.NewGuid();

            //controller.AddChanges(
            //    new LayerChange(
            //        BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Black), guid),
            //    new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Transparent), guid));

            //System.Tuple<LayerChange, LayerChange>[] changes = controller.PopChanges();
            //Assert.Equal(2, changes.Length);
        }

        [Fact]
        public void TestThatAddChangesAddsToExistingChange()
        {
            //Coordinates[] cords2 = { new Coordinates(2, 2), new Coordinates(5, 5) };
            //var data = CreateBasicController();
            //PixelChangesController controller = data.Item2;

            //controller.AddChanges(
            //    new LayerChange(
            //        BitmapPixelChanges.FromSingleColoredArray(cords2, SKColors.Black), data.Item1),
            //    new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords2, SKColors.Transparent), data.Item1));

            //Tuple<LayerChange, LayerChange>[] changes = controller.PopChanges();
            //Assert.Single(changes);
            //Assert.Equal(4, changes[0].Item1.PixelChanges.ChangedPixels.Count);
            //Assert.Equal(4, changes[0].Item2.PixelChanges.ChangedPixels.Count);
        }

        //private static Tuple<Guid, PixelChangesController> CreateBasicController()
        //{
        //    Coordinates[] cords = { new Coordinates(0, 0), new Coordinates(1, 1) };
        //    PixelChangesController controller = new PixelChangesController();

        //    Guid guid = Guid.NewGuid();

        //    controller.AddChanges(
        //        new LayerChange(
        //            BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Black), guid),
        //        new LayerChange(BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Transparent), guid));
        //    return new Tuple<Guid, PixelChangesController>(guid, controller);
        //}
    }
}
