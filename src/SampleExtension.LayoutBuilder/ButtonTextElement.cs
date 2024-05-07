using PixiEditor.Extensions.FlyUI.Elements;

namespace SampleExtension.LayoutBuilder;

public class ButtonTextElement : StatefulElement<ButtonTextElementState>
{
    public override ButtonTextElementState CreateState()
    {
        return new();
    }
}
