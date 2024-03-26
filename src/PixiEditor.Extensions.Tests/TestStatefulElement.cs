using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.Extensions.Test;

public class TestStatefulElement : StatefulElement<TestState>
{
    public override TestState CreateState()
    {
        return new();
    }
}
