using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ShaderNode : Node
{
    public InputProperty<Surface> Input { get; set; }
    public InputProperty<string> Shader { get; set; }
    public OutputProperty<Surface> Output { get; set; }
    
    private string _shaderCode = """
                                 half4 main(float2 coord) {
                                   float t = coord.x / 128;
                                   half4 white = half4(1);
                                   half4 black = half4(0,0,0,1);
                                   return mix(white, black, t);
                                 }
                                 """;
    
    public ShaderNode()
    {
        Input = CreateInput<Surface>("Input", "INPUT", null);
        Shader = CreateInput<string>("Shader", "SHADER", "");
        Output = CreateOutput<Surface>("Output", "OUTPUT", null);
    }

    protected override string NodeUniqueName { get; }

    public override string DisplayName { get; set; }

    protected override Surface? OnExecute(RenderingContext context)
    {
        if (Input.Value == null)
        {
            return null;
        }

        var shader =
            PixiEditor.DrawingApi.Core.Surface.PaintImpl.Shader.CreateFromSksl(_shaderCode, false, out string errors);
        if (shader == null)
        {
            Console.WriteLine(errors);
            return null;
        }

        Input.Value.DrawingSurface.Canvas.DrawPaint(new Paint {Shader = shader});
        return Input.Value;
    }

    public override Node CreateCopy()
    {
        return new ShaderNode();
    }
}
