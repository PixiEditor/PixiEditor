using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node
{
    private Pixmap? pixmap;

    public InputProperty<Surface?> Image { get; }
    
    public FuncOutputProperty<VecD> Coordinate { get; }
    
    public FuncOutputProperty<Color> Color { get; }

    public override string DisplayName { get; set; } = "MODIFY_IMAGE_LEFT_NODE";

    public ModifyImageLeftNode()
    {
        Image = CreateInput<Surface>(nameof(Surface), "IMAGE", null);
        Coordinate = CreateFieldOutput(nameof(Coordinate), "COORDINATE", ctx => ctx.Position);
        Color = CreateFieldOutput(nameof(Color), "COLOR", GetColor);
    }

    private Color GetColor(FuncContext context)
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
        pixmap = Image.Value?.PeekPixels();
    }

    protected override string NodeUniqueName => "ModifyImageLeft";

    protected override Surface? OnExecute(RenderingContext context)
    {
        return Image.Value;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new ModifyImageLeftNode();
}
