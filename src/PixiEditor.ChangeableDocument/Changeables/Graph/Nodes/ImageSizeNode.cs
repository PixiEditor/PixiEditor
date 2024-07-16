using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ImageSizeNode : Node
{
    public InputProperty<Surface?> Image { get; }
    
    public OutputProperty<VecI> Size { get; }
    
    public ImageSizeNode()
    {
        Image = CreateInput<Surface>(nameof(Image), "IMAGE", null);
        Size = CreateOutput(nameof(Size), "SIZE", new VecI());
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        Size.Value = Image.Value?.Size ?? new VecI();

        return null;
    }

    public override bool Validate() => Image.Value != null;

    public override Node CreateCopy() => new ImageSizeNode();
}
