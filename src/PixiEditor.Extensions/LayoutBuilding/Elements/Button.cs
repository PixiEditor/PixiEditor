using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Button : ISingleChildLayoutElement<Control>
{
    ILayoutElement<Control> ISingleChildLayoutElement<Control>.Child
    {
        get => Content;
        set => Content = value;
    }

    public ILayoutElement<Control> Content { get; set; }

    public Button(ILayoutElement<Control> content = null)
    {
        Content = content;
    }

    public Control Build()
    {
        return new Avalonia.Controls.Button() { Content = Content.Build() };
    }

    public void DeserializeProperties(List<object> values)
    {

    }
}
