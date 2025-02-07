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
    public InputProperty<double> FontSize { get; }
    public InputProperty<ShapeVectorData> OnPathData { get; }
    
    private string lastText = "";
    private VecD lastPosition = new VecD();
    private FontFamilyName lastFontFamily = new FontFamilyName();
    private double lastFontSize = 12d;
    private VectorPath? lastPath;

    private TextVectorData? cachedData;
    public TextNode()
    {
        Text = CreateInput("Text", "TEXT_LABEL", "");
        TextPosition = CreateInput("Position", "POSITION", new VecD());
        FontFamily = CreateInput("FontFamily", "FONT_LABEL", new FontFamilyName());
        FontSize = CreateInput("FontSize", "FONT_SIZE_LABEL", 12d);
        OnPathData = CreateInput<ShapeVectorData>("PathToDrawOn", "ON_PATH_DATA", null);
    }
    
    protected override TextVectorData? GetShapeData(RenderContext context)
    {
        string text = Text.Value;
        VecD position = TextPosition.Value;
        FontFamilyName fontFamily = FontFamily.Value;
        double fontSize = FontSize.Value;
        VectorPath? path = OnPathData.Value?.ToPath();
        
        if (text == lastText && position == lastPosition && fontFamily.Equals(lastFontFamily) && fontSize == lastFontSize && path == lastPath)
        {
            return cachedData;
        }
        
        lastText = text;
        lastPosition = position;
        lastFontFamily = fontFamily;
        lastFontSize = fontSize;
        lastPath = path;

        Font font = Font.FromFontFamily(fontFamily);
        if(font == null)
        {
            font = Font.CreateDefault();
        }
        
        font.Size = fontSize;
        
        cachedData = new TextVectorData()
        {
            Text = text,
            Position = position,
            Font = font,
            Path = path,
        };
        
        return cachedData;
    }

    public override Node CreateCopy()
    {
        return new TextNode();
    }
}
