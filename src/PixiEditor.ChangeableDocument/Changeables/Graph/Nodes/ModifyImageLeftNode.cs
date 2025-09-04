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
public class ModifyImageLeftNode : Node, IPairNode
{
    public InputProperty<Texture?> Image { get; }

    public FuncOutputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }
    
    public InputProperty<ColorSampleMode> SampleMode { get; }
    public InputProperty<bool> NormalizeCoordinates { get; }

    public Guid OtherNode { get; set; }
    
    public ModifyImageLeftNode()
    {
        Image = CreateInput<Texture?>("Surface", "IMAGE", null);
        Coordinate = CreateFuncOutput("Coordinate", "UV", ctx => ctx.OriginalPosition ?? new Float2(""));
        Color = CreateFuncOutput("Color", "COLOR", GetColor);
        SampleMode = CreateInput("SampleMode", "COLOR_SAMPLE_MODE", ColorSampleMode.ColorManaged);
        NormalizeCoordinates = CreateInput("NormalizeCoordinates", "NORMALIZE_COORDINATES", true);
    }
    
    private Half4 GetColor(FuncContext context)
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
        RenderPreviews(context);
    }

    public override Node CreateCopy() => new ModifyImageLeftNode();

    private void RenderPreviews(RenderContext context)
    {
        var previews = context.GetPreviewTexturesForNode(Id);
        if (previews is null) return;
        foreach (var request in previews)
        {
            var texture = request.Texture;
            if (texture is null) continue;

            int saved = texture.DrawingSurface.Canvas.Save();

            VecI size = Image.Value?.Size ?? context.RenderOutputSize;
            VecD scaling = PreviewUtility.CalculateUniformScaling(size, texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(size, texture.Size, scaling);
            texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            var previewCtx =
                PreviewUtility.CreatePreviewContext(context, scaling, context.RenderOutputSize, texture.Size);

            texture.DrawingSurface.Canvas.Clear();
            RenderPreview(texture.DrawingSurface);
            texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    public bool RenderPreview(DrawingSurface renderOn)
    {
        if(Image.Value is null)
        {
            return false;
        }

        renderOn.Canvas.DrawSurface(Image.Value.DrawingSurface, 0, 0); 
        return true;
    }
}
