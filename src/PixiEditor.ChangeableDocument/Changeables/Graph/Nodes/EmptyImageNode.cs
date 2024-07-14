using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class EmptyImageNode : Node
{
    public OutputProperty<ChunkyImage> Output { get; }

    public InputProperty<VecI> Size { get; }

    public EmptyImageNode()
    {
        Output = CreateOutput<ChunkyImage>(nameof(Output), "EMPTY_IMAGE", null);
        Size = CreateInput(nameof(Size), "SIZE", new VecI(32, 32));
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        Output.Value = new ChunkyImage(Size.Value);

        return Output.Value;
    }

    public override bool Validate() => Size.Value is { X: > 0, Y: > 0 };

    public override Node CreateCopy() => new EmptyImageNode();
}
