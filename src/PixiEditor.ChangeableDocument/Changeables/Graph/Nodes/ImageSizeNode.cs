using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSizeNode : Node
{
    public InputProperty<Image?> Image { get; }
    
    public OutputProperty<VecI> Size { get; }
    
    public ImageSizeNode()
    {
        Image = CreateInput<Image>(nameof(Image), "IMAGE", null);
        Size = CreateOutput(nameof(Size), "SIZE", new VecI());
    }
    
    protected override Image? OnExecute(KeyFrameTime frameTime)
    {
        Size.Value = Image.Value?.Size ?? new VecI();

        return null;
    }

    public override bool Validate() => Image.Value != null;

    public override Node CreateCopy() => new ImageSizeNode();
}
