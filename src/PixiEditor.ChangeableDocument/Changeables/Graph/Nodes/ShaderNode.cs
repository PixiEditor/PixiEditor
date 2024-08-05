using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Shader")]
public class ShaderNode : Node
{
    public override string DisplayName { get; set; } = "Shader Node";

    public InputProperty<Texture> Input { get; set; }
    public OutputProperty<Texture> Output { get; set; }
    

    private const string ShaderCode = @"
       half4 main(float2 coord)
       {
           return half4(1.0, 0.0, 0.0, 1.0); 
       } 
";

    private Paint paint = new Paint() { Shader = Shader.CreateFromSksl(ShaderCode, true, out _) };
    
    public ShaderNode()
    {
        Input = CreateInput<Texture>("Input", "INPUT", null);
        Output = CreateOutput<Texture>("Output", "OUTPUT", null);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        if(Input.Value is null)
            return null;
        
        Input.Value.Surface.Canvas.DrawPaint(paint);
        return Input.Value;
    }

    public override Node CreateCopy()
    {
        return new ShaderNode();
    }
}
