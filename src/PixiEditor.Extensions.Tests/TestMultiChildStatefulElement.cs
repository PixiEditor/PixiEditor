using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.Test;

public class TestMultiChildStatefulElement : StatefulElement<TestMultiChildState>
{
    public override TestMultiChildState CreateState()
    {
        return new TestMultiChildState();
    }
}
