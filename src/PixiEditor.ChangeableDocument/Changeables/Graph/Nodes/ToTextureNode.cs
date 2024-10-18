using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ToTexture")]
public class ToTextureNode : Node
{
    public RenderInputProperty RenderInput { get; }
    public InputProperty<VecI> Size { get; }
    public OutputProperty<Texture> Texture { get; }

    public ToTextureNode()
    {
        RenderInput = CreateRenderInput("RenderInput", "RENDER_INPUT");
        Size = CreateInput("Size", "SIZE", VecI.One).WithRules(x => 
            x.Min(VecI.One));
        Texture = CreateOutput<Texture>("Texture", "TEXTURE", null);
    }
    
    protected override void OnExecute(RenderContext context)
    {
        if(RenderInput.Value == null)
        {
            return;
        }

        Texture.Value = RequestTexture(0, Size.Value);
        RenderInput.Value.Paint(context, Texture.Value.DrawingSurface);
    }

    public override Node CreateCopy()
    {
        return new ToTextureNode();
    }
}
