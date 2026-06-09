using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Palette")]
public class PaletteNode : Node
{
    public InputProperty<Color[]> Colors { get; }
    public OutputProperty<Palette> Output { get; }

    public PaletteNode()
    {
        Colors = CreateInput<Color[]>("Colors", "COLORS", null);
        Output = CreateOutput<Palette>("Output", "OUTPUT", null);
    }
    protected override void OnExecute(RenderContext context)
    {
        Output.Value = new Palette(Colors.Value);
    }

    public override Node CreateCopy()
    {
        return new PaletteNode();
    }
}
