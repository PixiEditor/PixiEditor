using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class ShaderNode : Node
{
    public InputProperty<ChunkyImage?> Input { get; set; }
    public InputProperty<string> Shader { get; set; }
    public OutputProperty<ChunkyImage?> Output { get; set; }
    
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
        Input = CreateInput<ChunkyImage?>("Input", "INPUT", null);
        Shader = CreateInput<string>("Shader", "SHADER", "");
        Output = CreateOutput<ChunkyImage?>("Output", "OUTPUT", null);
    }
    
    protected override ChunkyImage? OnExecute(KeyFrameTime frameTime)
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

        ChunkyImage output = Input.Value.CloneFromCommitted();
        output.EnqueueDrawShader(shader);
        output.CommitChanges();
        
        Output.Value = output;
        return output;
    }

    public override bool Validate()
    {
        return true;
    }

    public override Node CreateCopy()
    {
        return new ShaderNode();
    }
}
