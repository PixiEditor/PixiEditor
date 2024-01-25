using Avalonia.Controls;
using PixiEditor.Extensions.LayoutBuilding;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.Extensions.Test;

public class LayoutBuilderTests
{
    [Fact]
    public void TestCenteredTextLayoutIsBuildCorrectly()
    {
        Layout layout = new Layout(
            body: new Center(
                child: new Text("Hello")));

        object result = layout.Build();

        Assert.IsType<Panel>(result);
        Panel grid = (Panel)result;
        Assert.Single(grid.Children);

        Assert.IsType<Panel>(grid.Children[0]);
        Panel childGrid = (Panel)grid.Children[0];

        Assert.Equal(Avalonia.Layout.HorizontalAlignment.Center, childGrid.HorizontalAlignment);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Center, childGrid.VerticalAlignment);

        Assert.Single(childGrid.Children);

        Assert.IsType<TextBlock>(childGrid.Children[0]);
        TextBlock textBlock = (TextBlock)childGrid.Children[0];

        Assert.Equal("Hello", textBlock.Text);
    }
}
