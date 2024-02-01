using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.Extensions.Test;

public class TestState : State
{
    public const string Format = "Clicked: {0}";
    public int ClickedTimes { get; private set; } = 0;
    public bool RemoveText { get; set; } = false;

    public override LayoutElement BuildElement()
    {
        return new Button(
            onClick: OnClick,
            child: RemoveText ? null : new Text(string.Format(Format, ClickedTimes)));
    }

    private void OnClick(ElementEventArgs args)
    {
        SetState(() => ClickedTimes++);
    }
}
