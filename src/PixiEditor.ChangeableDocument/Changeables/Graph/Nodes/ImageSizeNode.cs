using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSizeNode : Node
{
    public InputProperty<ChunkyImage?> Image { get; }
    
    public OutputProperty<VecI> Size { get; }
    
    public ImageSizeNode()
    {
        Image = CreateInput<ChunkyImage>(nameof(Image), "IMAGE", null);
        Size = CreateOutput(nameof(Size), "SIZE", new VecI());
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
    {
        Size.Value = Image.Value?.CommittedSize ?? new VecI();

        return null;
    }

    public override bool Validate() => Image.Value != null;

    public override Node CreateCopy() => new ImageSizeNode();
}
