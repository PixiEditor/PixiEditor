using PixiEditor.ChangeableDocument.Changeables.Graph.Palettes;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Palette")]
public class PaletteNode : Node
{
    public InputProperty<Palette> Colors { get; }
    public OutputProperty<Palette> Output { get; }

    public PaletteNode()
    {
        Colors = CreateInput<Palette>("Colors", "COLORS", Palette.empty);
        Output = CreateOutput<Palette>("Output", "OUTPUT", null);
    }
    protected override void OnExecute(RenderContext context)
    {
        Output.Value = Colors.Value ?? Palette.empty;
    }

    public override Node CreateCopy()
    {
        return new PaletteNode();
    }
}
