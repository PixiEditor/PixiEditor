using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.Extensions.Test;

public class TestNestedStatefulElement : StatefulElement<TestNestedState>
{
    public override TestNestedState CreateState()
    {
        return new();
    }
}
