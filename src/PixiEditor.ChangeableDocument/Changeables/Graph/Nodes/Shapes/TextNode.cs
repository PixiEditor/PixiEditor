using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("Text")]
public class TextNode : ShapeNode<TextVectorData>
{
    public InputProperty<string> Text { get; }
    public InputProperty<VecD> TextPosition { get; }
    public InputProperty<FontFamilyName> FontFamily { get; }
    public InputProperty<FontStyleWeight> FontWeight { get; }
    public InputProperty<FontStyleSlant> FontStyle { get; }
    public InputProperty<FontStyleWidth> FontStyleWidth { get; }

    public InputProperty<double> FontSize { get; }

    private string lastText = "";
    private VecD lastPosition = new VecD();
    private FontFamilyName lastFontFamily = new FontFamilyName();
    private FontStyleWeight lastFontWeight = FontStyleWeight.Normal;
    private FontStyleSlant lastSlant = FontStyleSlant.Upright;
    private FontStyleWidth lastWidth = Drawie.Backend.Core.Text.FontStyleWidth.Normal;

    private double lastFontSize = 12d;
    private VectorPath? lastPath;

    private TextVectorData? cachedData;

    public TextNode()
    {
        Text = CreateInput("Text", "TEXT_LABEL", "");
        TextPosition = CreateInput("Position", "POSITION", new VecD());
        FontFamily = CreateInput("FontFamily", "FONT_LABEL", new FontFamilyName());
        FontWeight = CreateInput("FontWeight", "FONT_WEIGHT_LABEL", FontStyleWeight.Normal);
        FontStyle = CreateInput("FontStyle", "FONT_STYLE_LABEL", FontStyleSlant.Upright);
        FontStyleWidth = CreateInput("FontStyleWidth", "FONT_WIDTH_LABEL",
            Drawie.Backend.Core.Text.FontStyleWidth.Normal);
        FontSize = CreateInput("FontSize", "FONT_SIZE_LABEL", 12d);
    }

    protected override TextVectorData? GetShapeData(RenderContext context)
    {
        string text = Text.Value;
        VecD position = TextPosition.Value;
        FontFamilyName fontFamily = FontFamily.Value;
        FontStyleWeight fontWeight = FontWeight.Value;
        FontStyleSlant fontStyle = FontStyle.Value;
        Drawie.Backend.Core.Text.FontStyleWidth fontStyleWidth = FontStyleWidth.Value;
        double fontSize = FontSize.Value;

        if (text == lastText && position == lastPosition && fontFamily.Equals(lastFontFamily) &&
            Math.Abs(fontSize - lastFontSize) < float.Epsilon && lastFontWeight == fontWeight &&
            lastSlant == fontStyle && lastWidth == fontStyleWidth)
        {
            return cachedData;
        }

        lastText = text;
        lastPosition = position;
        lastFontFamily = fontFamily;
        lastFontSize = fontSize;
        lastFontWeight = fontWeight;
        lastSlant = fontStyle;
        lastWidth = fontStyleWidth;

        FontData font = FontData.CreateDefault() with
        {
            Family = fontFamily, Size = fontSize, Weight = fontWeight, Slant = lastSlant, Width = fontStyleWidth
        };

        cachedData?.Text?.ClearCache();
        cachedData = new TextVectorData() { Text = new RichText(text, font), Position = position, };

        return cachedData;
    }

    public override Node CreateCopy()
    {
        return new TextNode();
    }
}
