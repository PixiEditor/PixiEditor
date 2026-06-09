using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("DecomposePalette")]
public class DecomposePaletteNode : Node
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
        if (Palette.Value != null)
        {
            Output.Value = Palette.Value.ToArray();
        }
        else
        {
            Output.Value = null;
        }
    }

    public override Node CreateCopy()
    {
        return new DecomposePaletteNode();
    }
}
