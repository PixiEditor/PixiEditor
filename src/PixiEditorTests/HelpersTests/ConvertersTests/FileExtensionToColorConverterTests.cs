using PixiEditor.Helpers;
using PixiEditor.Helpers.Converters;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Xunit;

namespace PixiEditorTests.HelpersTests.ConvertersTests;

public class FileExtensionToColorConverterTests
{
    private static SolidColorBrush GetTypedColor(string ext)
    {
        var converter = new FileExtensionToColorConverter();
        object value = converter.Convert(ext, typeof(int), null, CultureInfo.CurrentCulture);
        Assert.IsType<SolidColorBrush>(value);
        return value as SolidColorBrush;
    }

    [Fact]
    public void TestThatEachFormatHasColor()
    {
        SupportedFilesHelper.AllSupportedExtensions.ToList().ForEach(i =>
        {
            var typed = GetTypedColor(i);
            Assert.NotEqual(FileExtensionToColorConverter.UnknownBrush, typed);
        });
    }
               
    [Fact]
    public void TestThatUnsupportedFormatHasDefaultColor()
    {
        var converter = new FileExtensionToColorConverter();
        var typed = GetTypedColor(".abc");
        Assert.Equal(FileExtensionToColorConverter.UnknownBrush, typed);
    }
}