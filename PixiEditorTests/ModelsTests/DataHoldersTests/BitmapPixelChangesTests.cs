using System.Windows.Media;
using PixiEditor.Exceptions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditorTests.ModelsTests.ColorsTests;
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
            BitmapPixelChanges bmpChanges = BitmapPixelChanges.FromSingleColoredArray(cords, ExtendedColorTests.green);

            Assert.All(bmpChanges.ChangedPixels.Values, changeColor => Assert.Equal(ExtendedColorTests.green, changeColor));
            Assert.True(bmpChanges.WasBuiltAsSingleColored);
        }

        [Fact]
        public void TestThatCombineCombineOverrideCombinesValues()
        {
            Coordinates[] cords1 = { new Coordinates(0, 0), new Coordinates(1, 0), new Coordinates(3, 2) };
            Coordinates[] cords2 = { new Coordinates(3, 2), new Coordinates(0, 0), new Coordinates(5, 5) };
            BitmapPixelChanges changes = BitmapPixelChanges.FromSingleColoredArray(cords1, ExtendedColorTests.green);
            BitmapPixelChanges changes2 = BitmapPixelChanges.FromSingleColoredArray(cords2, ExtendedColorTests.red);

            BitmapPixelChanges output = BitmapPixelChanges.CombineOverride(new[] { changes, changes2 });
            Assert.Equal(4, output.ChangedPixels.Count);
            Assert.Equal(ExtendedColorTests.red, output.ChangedPixels[new Coordinates(3, 2)]);
            Assert.Equal(ExtendedColorTests.black, output.ChangedPixels[new Coordinates(0, 0)]);
            Assert.Equal(ExtendedColorTests.green, output.ChangedPixels[new Coordinates(1, 0)]);
        }

        [Fact]
        public void TestThatFromArraysThrowsError()
        {
            Assert.Throws<ArrayLengthMismatchException>(
                () => BitmapPixelChanges.FromArrays(new[] { new Coordinates(0, 0) }, new[] { ExtendedColorTests.red, ExtendedColorTests.green }));
        }

        [Fact]
        public void TestThatFormArraysWorks()
        {
            Coordinates[] coordinatesArray = { new Coordinates(0, 0), new Coordinates(2, 3), new Coordinates(5, 5) };
            SKColor[] colorsArray = { ExtendedColorTests.red, ExtendedColorTests.green, ExtendedColorTests.blue };
            BitmapPixelChanges result = BitmapPixelChanges.FromArrays(coordinatesArray, colorsArray);
            for (int i = 0; i < coordinatesArray.Length; i++)
            {
                Coordinates cords = coordinatesArray[i];
                Assert.Equal(colorsArray[i], result.ChangedPixels[cords]);
            }

            Assert.False(result.WasBuiltAsSingleColored);
        }
    }
}