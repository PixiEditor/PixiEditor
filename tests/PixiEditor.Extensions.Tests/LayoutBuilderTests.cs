using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.UI.Panels;
using Button = PixiEditor.Extensions.FlyUI.Elements.Button;

namespace PixiEditor.Extensions.Test;

[Collection("LayoutBuilderTests")]
public class LayoutBuilderTests
{
    [Fact]
    public void TestThatButtonClickEventFiresCallback()
    {
        Button button = new Button();
        bool callbackFired = false;

        button.Click += (e) => callbackFired = true;
        button.RaiseEvent(nameof(Button.Click), new ElementEventArgs());

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

        testStatefulElement.State.SetState(() =>
        {
            testStatefulElement.State.ReplaceText = true;
            testStatefulElement.State.ReplaceTextWith = null;
        });

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

        testStatefulElement.State.SetState(() =>
        {
            testStatefulElement.State.ReplaceText = true;
            testStatefulElement.State.ReplaceTextWith = null;
        });

        Assert.Null(button.Content); // Old layout is updated and text is removed

        testStatefulElement.State.SetState(() => testStatefulElement.State.ReplaceText = false);

        Assert.NotNull(button.Content); // Old layout is updated and text is added
        Assert.IsType<TextBlock>(button.Content);
    }

    [Fact]
    public void TestStateReplacesChildInTree()
    {
        TestStatefulElement testStatefulElement = new TestStatefulElement();
        testStatefulElement.CreateState();
        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<Avalonia.Controls.Button>((native as ContentPresenter).Content);
        Avalonia.Controls.Button button = (native as ContentPresenter).Content as Avalonia.Controls.Button;

        Assert.NotNull(button.Content);
        Assert.IsType<TextBlock>(button.Content);

        testStatefulElement.State.SetState(() =>
        {
            testStatefulElement.State.ReplaceText = true;
            testStatefulElement.State.ReplaceTextWith = new Button();
        });

        Assert.IsType<Avalonia.Controls.Button>(button.Content); // Old layout is updated and text is removed

        testStatefulElement.State.SetState(() => testStatefulElement.State.ReplaceText = false);

        Assert.NotNull(button.Content); // Old layout is updated and text is added
        Assert.IsType<TextBlock>(button.Content);
    }

    [Fact]
    public void TestThatMultiChildLayoutStateUpdatesTreeCorrectly()
    {
        TestMultiChildStatefulElement testStatefulElement = new TestMultiChildStatefulElement();
        testStatefulElement.CreateState();

        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<ColumnPanel>((native as ContentPresenter).Content);
        ColumnPanel panel = (native as ContentPresenter).Content as ColumnPanel;

        Assert.Equal(2, panel.Children.Count);

        Assert.IsType<Avalonia.Controls.Button>(panel.Children[0]);
        Assert.IsType<RowPanel>(panel.Children[1]);

        Assert.Empty((panel.Children[1] as RowPanel).Children);
        Assert.Empty(testStatefulElement.State.Rows);

        Avalonia.Controls.Button button = (Avalonia.Controls.Button)panel.Children[0];
        RowPanel innerPanel = (RowPanel)panel.Children[1];

        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Single(innerPanel.Children);
        Assert.Single(testStatefulElement.State.Rows);
        Assert.IsType<TextBlock>(innerPanel.Children[0]);

        button.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));

        Assert.Equal(2, innerPanel.Children.Count);
    }

    [Fact]
    public void TestThatNestedStatefulElementsAreUpdatedCorrectly()
    {
        //TODO: Make this test
        /*TestNestedStatefulElement testStatefulElement = new TestNestedStatefulElement();
        testStatefulElement.CreateState();

        var native = testStatefulElement.BuildNative();

        Assert.IsType<ContentPresenter>(native);
        Assert.IsType<ContentPresenter>((native as ContentPresenter).Content);
        ContentPresenter innerPresenter = (native as ContentPresenter).Content as ContentPresenter;

        Assert.IsType<Avalonia.Controls.Button>(innerPresenter.Content);
        Avalonia.Controls.Button button = (innerPresenter.Content as Avalonia.Controls.Button);

        Assert.IsType<TextBlock>(button.Content);
        TextBlock textBlock = button.Content as TextBlock;*/
    }
}
