using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageLeft")]
[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node, IPairNode, IPreviewRenderable
{
    public InputProperty<Texture?> Image { get; }

    public FuncOutputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public Guid OtherNode { get; set; }
    
    public ModifyImageLeftNode()
    {
        Image = CreateInput<Texture?>("Surface", "IMAGE", null);
        Coordinate = CreateFuncOutput("Coordinate", "UV", ctx => ctx.OriginalPosition);
        Color = CreateFuncOutput("Color", "COLOR", GetColor);
    }
    
    private Half4 GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        return context.SampleSurface(Image.Value.DrawingSurface, context.SamplePosition);
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

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        if(Image.Value is null)
        {
            return false;
        }

        RenderContext renderContext = new(renderOn, frame, ChunkResolution.Full, VecI.Zero);
        renderOn.Canvas.DrawSurface(Image.Value.DrawingSurface, 0, 0); 
        return true;
    }
}
