using System.Globalization;
using PixiEditor.Helpers.Converters;
using Xunit;

namespace PixiEditorTests.HelpersTests.ConvertersTests
{
    public class DoubleToIntConverterTest
    {
        [Fact]
        public void TestThatConvertConvertsDoubleToInt()
        {
            var converter = new DoubleToIntConverter();

            var value = converter.Convert(5.123, typeof(int), null, CultureInfo.CurrentCulture);

            Assert.IsType<int>(value);
            Assert.Equal(5, (int) value);
        }
    }
}