using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
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
    
    protected override Image? OnExecute(KeyFrameTime frameTime)
    {
        using var surface = new Surface(Size.Value);

        return surface.DrawingSurface.Snapshot();
    }

    public override bool Validate() => Size.Value is { X: > 0, Y: > 0 };

    public override Node CreateCopy() => new EmptyImageNode();
}
