using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using SkiaSharp;
using Xunit;

namespace PixiEditorTests.ModelsTests.DataHoldersTests
{
    public class BitmapPixelChangesTests
    {
        [Fact]
        public void TestThatFromSingleColoredArrayCreatesCorrectArray()
        {
            Coordinates[] cords = { new Coordinates(0, 0), new Coordinates(1, 0), new Coordinates(3, 2) };
            BitmapPixelChanges bmpChanges = BitmapPixelChanges.FromSingleColoredArray(cords, SKColors.Lime);

            Assert.All(bmpChanges.ChangedPixels.Values, changeColor => Assert.Equal(SKColors.Lime, changeColor));
            Assert.True(bmpChanges.WasBuiltAsSingleColored);
        }

        [Fact]
        public void TestThatCombineCombineOverrideCombinesValues()
        {
            Coordinates[] cords1 = { new Coordinates(0, 0), new Coordinates(1, 0), new Coordinates(3, 2) };
            Coordinates[] cords2 = { new Coordinates(3, 2), new Coordinates(0, 0), new Coordinates(5, 5) };
            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(cords1, SKColors.Lime);
            BitmapPixelChanges changes2 = BitmapPixelChanges.FromSingleColoredArray(cords2, SKColors.Red);

            BitmapPixelChanges output = BitmapPixelChanges.CombineOverride(new[] { changes, changes2 });
            Assert.Equal(4, output.ChangedPixels.Count);
            Assert.Equal(SKColors.Red, output.ChangedPixels[new Coordinates(3, 2)]);
            Assert.Equal(SKColors.Red, output.ChangedPixels[new Coordinates(0, 0)]);
            Assert.Equal(SKColors.Lime, output.ChangedPixels[new Coordinates(1, 0)]);
        }
    }
}
