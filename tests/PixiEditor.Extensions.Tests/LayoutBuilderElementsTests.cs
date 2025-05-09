using Avalonia.Controls;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.UI.Panels;

namespace PixiEditor.Extensions.Test;

[Collection("LayoutBuilderTests")]
public class LayoutBuilderElementsTests
{
    [Fact]
    public void TestThatRowLayoutIsBuildCorrectly()
    {
        Layout layout = new Layout(
            body: new Row(
                new Text("Hello"),
                new Text("World")));

        Control result = layout.BuildNative();

        Assert.IsType<Panel>(result);
        Panel grid = (Panel)result;
        Assert.Single(grid.Children);

        Assert.IsType<RowPanel>(grid.Children[0]);
        Panel childGrid = (RowPanel)grid.Children[0];

        Assert.Equal(Avalonia.Layout.HorizontalAlignment.Stretch, childGrid.HorizontalAlignment);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Stretch, childGrid.VerticalAlignment);

        Assert.Equal(2, childGrid.Children.Count);

        Assert.IsType<TextBlock>(childGrid.Children[0]);
        TextBlock textBlock = (TextBlock)childGrid.Children[0];

        Assert.Equal("Hello", textBlock.Text);

        Assert.IsType<TextBlock>(childGrid.Children[1]);
        TextBlock textBlock2 = (TextBlock)childGrid.Children[1];

        Assert.Equal("World", textBlock2.Text);
    }

    [Fact]
    public void TestThatColumnLayoutIsBuildCorrectly()
    {
        Layout layout = new Layout(
            body: new Column(
                new Text("Hello"),
                new Text("World")));

        Control result = layout.BuildNative();

        Assert.IsType<Panel>(result);
        Panel grid = (Panel)result;
        Assert.Single(grid.Children);

        Assert.IsType<ColumnPanel>(grid.Children[0]);
        Panel childGrid = (ColumnPanel)grid.Children[0];

        Assert.Equal(Avalonia.Layout.HorizontalAlignment.Stretch, childGrid.HorizontalAlignment);
        Assert.Equal(Avalonia.Layout.VerticalAlignment.Stretch, childGrid.VerticalAlignment);

        Assert.Equal(2, childGrid.Children.Count);

        Assert.IsType<TextBlock>(childGrid.Children[0]);
        TextBlock textBlock = (TextBlock)childGrid.Children[0];

        Assert.Equal("Hello", textBlock.Text);

        Assert.IsType<TextBlock>(childGrid.Children[1]);
        TextBlock textBlock2 = (TextBlock)childGrid.Children[1];

        Assert.Equal("World", textBlock2.Text);
    }

    [Fact]
    public void TestCenteredTextLayoutIsBuildCorrectly()
    {
        Layout layout = new Layout(
            body: new Center(
                child: new Text("Hello")));

        object result = layout.BuildNative();

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
