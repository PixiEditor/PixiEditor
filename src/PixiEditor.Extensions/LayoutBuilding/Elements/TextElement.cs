using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class TextElement(string value = "") : LayoutElement, ITextElement<Control>
{
    public string Value { get; set; } = value;
    public abstract override Control Build();
}
