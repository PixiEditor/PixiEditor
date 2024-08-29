using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CreateImage", "CREATE_IMAGE_NODE", Category = "IMAGE")]
public class CreateImageNode : Node
{
    private Paint _paint = new();
    
    public OutputProperty<Texture> Output { get; }

    public InputProperty<VecI> Size { get; }
    
    public InputProperty<Color> Fill { get; }

    public CreateImageNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32)).WithRules(v => v.Min(VecI.One));
        Fill = CreateInput(nameof(Fill), "FILL", new Color(0, 0, 0, 255));
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return null;
        }

        var surface = RequestTexture(0, Size.Value); 

        _paint.Color = Fill.Value;
        surface.DrawingSurface.Canvas.DrawPaint(_paint);

        Output.Value = surface;

        return Output.Value;
    }
 
    public override Node CreateCopy() => new CreateImageNode();
}
