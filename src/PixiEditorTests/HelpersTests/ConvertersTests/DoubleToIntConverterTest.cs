using System.Globalization;
using PixiEditor.Helpers.Converters;
using Xunit;

namespace PixiEditorTests.Helpers;

public class DoubleToIntConverterTest
{
    [Fact]
    public void TestThatConvertConvertsDoubleToInt()
    {
        DoubleToIntConverter converter = new DoubleToIntConverter();

        object value = converter.Convert(5.123, typeof(int), null, CultureInfo.CurrentCulture);

        Assert.IsType<int>(value);
        Assert.Equal(5, (int)value);
    }
}