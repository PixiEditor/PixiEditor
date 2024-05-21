using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.Test;

public class TestStatefulElement : StatefulElement<TestState>
{
    public override TestState CreateState()
    {
        return new();
    }
}
