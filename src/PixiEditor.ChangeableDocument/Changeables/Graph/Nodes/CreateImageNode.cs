using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CreateImage")]
public class CreateImageNode : Node
{
    public OutputProperty<Texture> Output { get; }

    public InputProperty<VecI> Size { get; }

    public InputProperty<Color> Fill { get; }

    public RenderInputProperty Content { get; }

    public CreateImageNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32)).WithRules(v => v.Min(VecI.One));
        Fill = CreateInput(nameof(Fill), "FILL", Colors.Transparent);
        Content = CreateRenderInput(nameof(Content), "CONTENT");
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Size.Value.X <= 0 || Size.Value.Y <= 0)
        {
            return;
        }
        
        var surface = RequestTexture(0, Size.Value, false);

        surface.DrawingSurface.Canvas.Clear(Fill.Value);

        Content.Value?.Paint(context, surface.DrawingSurface);

        Output.Value = surface;
    }

    public override Node CreateCopy() => new CreateImageNode();
}
