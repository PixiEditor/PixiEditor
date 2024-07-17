using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ModifyImageLeftNode : Node
{
    private Pixmap? pixmap;
    
    public InputProperty<ChunkyImage?> Image { get; }
    
    public FieldOutputProperty<VecD> Coordinate { get; }
    
    public FieldOutputProperty<Color> Color { get; }
    
    public ModifyImageLeftNode()
    {
        Image = CreateInput<ChunkyImage>(nameof(Surface), "IMAGE", null);
        Coordinate = CreateFieldOutput(nameof(Coordinate), "COORDINATE", ctx => ctx.Position);
        Color = CreateFieldOutput(nameof(Color), "COLOR", GetColor);
    }

    private Color GetColor(FieldContext context)
    {
        context.ThrowOnMissingContext();
        
        if (pixmap == null)
            return new Color();
        
        var x = context.Position.X * context.Size.X;
        var y = context.Position.Y * context.Size.Y;
        
        return pixmap.GetPixelColor((int)x, (int)y);
    }

    internal void PreparePixmap()
    {
        //pixmap = Image.Value?.PeekPixels();
    }

    protected override Chunk? OnExecute(RenderingContext context)
    {
        return null;
        //return Image.Value;
    }

    public override bool Validate() => Image.Connection != null;

    public override Node CreateCopy() => new ModifyImageLeftNode();
}
