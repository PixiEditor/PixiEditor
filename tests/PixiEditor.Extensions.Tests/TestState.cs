using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.Test;

public class TestState : State
{
    public const string Format = "Clicked: {0}";
    public int ClickedTimes { get; private set; } = 0;
    public bool ReplaceText { get; set; } = false;
    public LayoutElement? ReplaceTextWith { get; set; } = null;

    public override LayoutElement BuildElement()
    {
        return new Button(
            onClick: OnClick,
            child: ReplaceText ? ReplaceTextWith : new Text(string.Format(Format, ClickedTimes)));
    }

    private void OnClick(ElementEventArgs args)
    {
        SetState(() => ClickedTimes++);
    }
}
