using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.Test;

public class TestNestedState : State
{
    public override LayoutElement BuildElement()
    {
        return new TestStatefulElement();
    }
}
