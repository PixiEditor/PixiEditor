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
    public InputProperty<string> FontFamily { get; }
    public InputProperty<double> FontSize { get; }
    public InputProperty<ShapeVectorData> OnPathData { get; }
    
    private string lastText = "";
    private VecD lastPosition = new VecD();
    private string lastFontFamily = "";
    private double lastFontSize = 12d;
    private VectorPath? lastPath;

    private TextVectorData? cachedData;
    public TextNode()
    {
        Text = CreateInput("Text", "TEXT", "");
        TextPosition = CreateInput("Position", "POSITION", new VecD());
        FontFamily = CreateInput("FontFamily", "FONT_FAMILY", "");
        FontSize = CreateInput("FontSize", "FONT_SIZE", 12d);
        OnPathData = CreateInput<ShapeVectorData>("PathToDrawOn", "ON_PATH_DATA", null);
    }
    
    protected override TextVectorData? GetShapeData(RenderContext context)
    {
        string text = Text.Value;
        VecD position = TextPosition.Value;
        string fontFamily = FontFamily.Value;
        double fontSize = FontSize.Value;
        VectorPath? path = OnPathData.Value?.ToPath();
        
        if (text == lastText && position == lastPosition && fontFamily == lastFontFamily && fontSize == lastFontSize && path == lastPath)
        {
            return cachedData;
        }
        
        lastText = text;
        lastPosition = position;
        lastFontFamily = fontFamily;
        lastFontSize = fontSize;
        lastPath = path;

        Font font = Font.FromFamilyName(fontFamily);
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

    public override void Dispose()
    {
        base.Dispose();
        cachedData?.Font.Dispose();
    }
}
