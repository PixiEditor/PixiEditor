using SkiaSharp;

namespace Nodes.Nodes;

public class LayerNode : Node
{
    public InputProperty<SKSurface?> Background { get; }
    public OutputProperty<SKSurface> Output { get; }
    public SKSurface LayerImage { get; set; }
    
    public LayerNode(string name, SKSizeI size) : base(name)
    {
        Background = CreateInput<SKSurface>("Background", null);
        Output = CreateOutput<SKSurface>("Image", SKSurface.Create(new SKImageInfo(size.Width, size.Height)));
        LayerImage = SKSurface.Create(new SKImageInfo(size.Width, size.Height));
    }

    public override bool Validate()
    {
        return true;
    }

    public override void OnExecute(int frame)
    {
        using SKPaint paint = new SKPaint();
        if (Background.Value != null)
        {
            Output.Value.Draw(Background.Value.Canvas, 0, 0, paint);       
        }
        
        Output.Value.Canvas.DrawSurface(LayerImage, 0, 0, paint);
    }

   
}
