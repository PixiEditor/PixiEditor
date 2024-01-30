using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.LayoutBuilding;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using Button = PixiEditor.Extensions.LayoutBuilding.Elements.Button;

namespace PixiEditor.Extensions.Test;

public class LayoutBuilderTests
{
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

    [Fact]
    public void TestThatButtonClickEventFiresCallback()
    {
        Button button = new Button();
        bool callbackFired = false;

        button.Click += (e) => callbackFired = true;
        button.RaiseEvent(nameof(Button.Click), ElementEventArgs.Empty);

        Assert.True(callbackFired);
    }

    [Fact]
    public void TestThatAvaloniaClickEventFiresElementCallback()
    {
        Button button = new Button();
        bool callbackFired = false;

        button.Click += (e) => callbackFired = true;

        button.BuildNative().RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.True(callbackFired);
    }

    [Fact]
    public void TestStateChangesDataAndOnlyAppliesDiffProperties()
    {
        TestStatefulElement testStatefulElement = new TestStatefulElement();
        testStatefulElement.CreateState();
        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<Avalonia.Controls.Button>((native as ContentPresenter).Content);
        Avalonia.Controls.Button button = (native as ContentPresenter).Content as Avalonia.Controls.Button;
        Assert.IsType<TextBlock>(button.Content);

        TextBlock textBlock = button.Content as TextBlock;

        Assert.Equal(0, testStatefulElement.State.ClickedTimes);
        Assert.Equal(string.Format(TestState.Format, 0), textBlock.Text);

        ContentPresenter contentPresenter = native as ContentPresenter;

        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Equal(1, testStatefulElement.State.ClickedTimes);

        Assert.IsType<ContentPresenter>(native);
        Assert.Equal(contentPresenter, native);
        Assert.IsType<Avalonia.Controls.Button>(contentPresenter.Content);
        Assert.Equal(button, contentPresenter.Content);
        Assert.IsType<TextBlock>(button.Content);
        Assert.Equal(textBlock, button.Content);
        Assert.Equal(string.Format(TestState.Format, 1), textBlock.Text);
    }

    [Fact]
    public void TestStateRemovesChildFromTree()
    {
        TestStatefulElement testStatefulElement = new TestStatefulElement();
        testStatefulElement.CreateState();
        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<Avalonia.Controls.Button>((native as ContentPresenter).Content);
        Avalonia.Controls.Button button = (native as ContentPresenter).Content as Avalonia.Controls.Button;

        Assert.NotNull(button.Content);
        Assert.IsType<TextBlock>(button.Content);

        testStatefulElement.State.SetState(() => testStatefulElement.State.RemoveText = true);

        Assert.Null(button.Content); // Old layout is updated and text is removed
    }

    [Fact]
    public void TestStateAddsChildToTree()
    {
        TestStatefulElement testStatefulElement = new TestStatefulElement();
        testStatefulElement.CreateState();
        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<Avalonia.Controls.Button>((native as ContentPresenter).Content);
        Avalonia.Controls.Button button = (native as ContentPresenter).Content as Avalonia.Controls.Button;

        Assert.NotNull(button.Content);
        Assert.IsType<TextBlock>(button.Content);

        testStatefulElement.State.SetState(() => testStatefulElement.State.RemoveText = true);

        Assert.Null(button.Content); // Old layout is updated and text is removed

        testStatefulElement.State.SetState(() => testStatefulElement.State.RemoveText = false);

        Assert.NotNull(button.Content); // Old layout is updated and text is added
        Assert.IsType<TextBlock>(button.Content);
    }
}
