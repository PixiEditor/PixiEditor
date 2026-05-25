using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
[NodeInfo("ColorRampNode")]
public class ColorRampNode : Node
{
    public InputProperty<Texture> Fac { get; }
    public OutputProperty<Texture> Image { get; }
    public OutputProperty<Texture> Alpha { get; }
    public ColorRampNode()
    {
        Fac = CreateInput<Texture>(nameof(Fac), "FAC", null);

        Image = CreateOutput<Texture>(nameof(Image), "IMAGE", null);
        Alpha = CreateOutput<Texture>(nameof(Alpha), "ALPHA", null);
    }
    protected override void OnExecute(RenderContext context)
    {
        
    }
    public override Node CreateCopy()
    {
        return new ColorRampNode();
    }
}
