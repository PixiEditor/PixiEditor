using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("DecomposePalette")]
public class DecomposePaletteNode : Node, IIterativeRenderSupport
{
    public InputProperty<Palette> Palette { get; }
    public OutputProperty<Color[]> Output { get; }

    public DecomposePaletteNode()
    {
        Palette = CreateInput<Palette>("Palette", "PALETTE", null);
        Output = CreateOutput<Color[]>("Output", "OUTPUT", null);
    }
    protected override void OnExecute(RenderContext context)
    {
        Output.Value = Palette.Value?.ToArray() ?? Array.Empty<Color>();
    }

    public override Node CreateCopy()
    {
        return new DecomposePaletteNode();
    }

    bool IIterativeRenderSupport.SupportsIterativeRendering => true;
}
