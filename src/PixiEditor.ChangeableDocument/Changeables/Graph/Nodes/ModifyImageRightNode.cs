using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.DrawingApi.Core.Shaders.Generation;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageRight")]
[PairNode(typeof(ModifyImageLeftNode), "ModifyImageZone")]
public class ModifyImageRightNode : RenderNode, IPairNode, ICustomShaderNode
{
    public Guid OtherNode { get; set; }

    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };

    public FuncInputProperty<Float2> Coordinate { get; }
    public FuncInputProperty<Half4> Color { get; }


    private string _lastSksl;


    public ModifyImageRightNode()
    {
        Coordinate = CreateFuncInput(nameof(Coordinate), "UV", new Float2("coords"));
        Color = CreateFuncInput(nameof(Color), "COLOR", new Half4(""));
    }

    protected override void OnPaint(RenderContext renderContext, DrawingSurface targetSurface)
    {
        if (OtherNode == null)
        {
            FindStartNode();
            if (OtherNode == null)
            {
                return;
            }
        }

        var startNode = FindStartNode();
        if (startNode == null)
        {
            return;
        }

        if (startNode.InputTexture is not { Size: var size })
        {
            return;
        }

        var width = size.X;
        var height = size.Y;

        Texture surface = RequestTexture(0, size);

        ShaderBuilder builder = new(size);
        FuncContext context = new(renderContext, builder);

        if (Coordinate.Connection != null)
        {
            var coordinate = Coordinate.Value(context);
            if (string.IsNullOrEmpty(coordinate.VariableName))
            {
                builder.SetConstant(context.SamplePosition, coordinate);
            }
            else
            {
                builder.Set(context.SamplePosition, coordinate);
            }
        }
        else
        {
            var constCoords = Coordinate.NonOverridenValue(FuncContext.NoContext);
            constCoords.VariableName = "constCords";
            builder.AddUniform(constCoords.VariableName, constCoords.ConstantValue);
            builder.Set(context.SamplePosition, constCoords);
        }

        if (Color.Connection != null)
        {
            builder.ReturnVar(Color.Value(context));
        }
        else
        {
            Half4 color = Color.NonOverridenValue(FuncContext.NoContext);
            color.VariableName = "color";
            builder.AddUniform(color.VariableName, color.ConstantValue);
            builder.ReturnVar(color);
        }

        string sksl = builder.ToSkSl();
        if (sksl != _lastSksl)
        {
            _lastSksl = sksl;
            drawingPaint?.Shader?.Dispose();
            drawingPaint.Shader = builder.BuildShader();
        }
        else
        {
            drawingPaint.Shader = drawingPaint.Shader.WithUpdatedUniforms(builder.Uniforms);
        }

        surface.DrawingSurface.Canvas.DrawPaint(drawingPaint);

        targetSurface.Canvas.DrawSurface(surface.DrawingSurface, 0, 0);
        builder.Dispose();
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        return false;
    }

    public override void Dispose()
    {
        base.Dispose();
        drawingPaint?.Dispose();
    }

    private ModifyImageLeftNode FindStartNode()
    {
        ModifyImageLeftNode startNode = null;
        TraverseBackwards(node =>
        {
            if (node is ModifyImageLeftNode leftNode)
            {
                startNode = leftNode;
                OtherNode = leftNode.Id;
                return false;
            }

            return true;
        });

        return startNode;
    }

    public override Node CreateCopy() => new ModifyImageRightNode();
}
