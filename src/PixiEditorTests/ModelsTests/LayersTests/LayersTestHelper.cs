using PixiEditor.Models.Layers;
using Xunit;

namespace PixiEditorTests.ModelsTests.LayersTests;

public static class LayersTestHelper
{
    public static void LayersAreEqual(Layer expected, Layer actual)
    {
        Assert.NotNull(actual);
        Assert.NotNull(expected);
#pragma warning disable CA1062 // Validate arguments of public methods
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Offset, actual.Offset);
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.MaxHeight, actual.MaxHeight);
        Assert.Equal(expected.MaxWidth, actual.MaxWidth);
        Assert.Equal(expected.Opacity, actual.Opacity);
        Assert.Equal(expected.IsVisible, actual.IsVisible);
        Assert.Equal(expected.IsRenaming, actual.IsRenaming);
        Assert.Equal(expected.ConvertBitmapToBytes(), actual.ConvertBitmapToBytes());
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}