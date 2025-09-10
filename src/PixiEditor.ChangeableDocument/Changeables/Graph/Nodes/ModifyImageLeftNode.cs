using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Shaders.Generation;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageLeft")]
[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node, IPairNode, IPreviewRenderable
{
    public InputProperty<Texture?> Image { get; }

    public FuncOutputProperty<Float2, ShaderFuncContext> Coordinate { get; }

    public FuncOutputProperty<Half4, ShaderFuncContext> Color { get; }
    
    public InputProperty<ColorSampleMode> SampleMode { get; }
    public InputProperty<bool> NormalizeCoordinates { get; }

    public Guid OtherNode { get; set; }
    
    public ModifyImageLeftNode()
    {
        Image = CreateInput<Texture?>("Surface", "IMAGE", null);
        Coordinate = CreateFuncOutput<Float2, ShaderFuncContext>("Coordinate", "UV", ctx => ctx.OriginalPosition ?? new Float2(""));
        Color = CreateFuncOutput<Half4, ShaderFuncContext>("Color", "COLOR", GetColor);
        SampleMode = CreateInput("SampleMode", "COLOR_SAMPLE_MODE", ColorSampleMode.ColorManaged);
        NormalizeCoordinates = CreateInput("NormalizeCoordinates", "NORMALIZE_COORDINATES", true);
    }
    
    private Half4 GetColor(ShaderFuncContext context)
    {
        context.ThrowOnMissingContext();
        
        if(Image.Value == null)
        {
            return new Half4("") { ConstantValue = Colors.Transparent };
        }

        return context.SampleSurface(Image.Value.DrawingSurface, context.SamplePosition, SampleMode.Value, NormalizeCoordinates.Value);
    }

    protected override void OnExecute(RenderContext context)
    {
    }

    public override Node CreateCopy() => new ModifyImageLeftNode();
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if(Image.Value == null)
        {
            return null;
        } 
        
        return new RectD(0, 0, Image.Value.Size.X, Image.Value.Size.Y);
    }

    public bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if(Image.Value is null)
        {
            return false;
        }

        renderOn.Canvas.DrawSurface(Image.Value.DrawingSurface, 0, 0); 
        return true;
    }
}
