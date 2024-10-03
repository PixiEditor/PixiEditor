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
public class ModifyImageRightNode : Node, IPairNode, ICustomShaderNode
{
    public Guid OtherNode { get; set; }

    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };

    public FuncInputProperty<Float2> Coordinate { get; }
    public FuncInputProperty<Half4> Color { get; }

    public OutputProperty<Texture> Output { get; }


    private string _lastSksl;
    

    public ModifyImageRightNode()
    {
        Coordinate = CreateFuncInput(nameof(Coordinate), "UV", new Float2("coords"));
        Color = CreateFuncInput(nameof(Color), "COLOR", new Half4(""));
        Output = CreateOutput<Texture>(nameof(Output), "OUTPUT", null);
    }

    protected override void OnExecute(RenderContext renderContext)
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
        
        if (startNode.Image.Value is not { Size: var size })
        {
            return;
        }

        var width = size.X;
        var height = size.Y;

        Texture surface = RequestTexture(0, size);

        if (!surface.IsHardwareAccelerated)
        {
            startNode.PreparePixmap(renderContext);

            using Pixmap targetPixmap = surface.PeekReadOnlyPixels();

            ModifyImageInParallel(renderContext, targetPixmap, width, height);

            startNode.DisposePixmap(renderContext);
        }
        else
        {
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
            builder.Dispose();
        }
        
        Output.Value = surface;
    }

    private unsafe void ModifyImageInParallel(RenderContext renderContext, Pixmap targetPixmap, int width,
        int height)
    {
        int threads = Environment.ProcessorCount;
        int chunkHeight = height / threads;

        /*Parallel.For(0, threads, i =>
        {
            FuncContext context = new(renderingContext);

            int startY = i * chunkHeight;
            int endY = (i + 1) * chunkHeight;
            if (i == threads - 1)
            {
                endY = height;
            }

            Half* drawArray = (Half*)targetPixmap.GetPixels();

            for (int y = startY; y < endY; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    context.UpdateContext(new VecD((double)x / width, (double)y / height), new VecI(width, height));
                    var coordinate = Coordinate.Value(context);
                    context.UpdateContext(coordinate, new VecI(width, height));

                    var color = Color.Value(context);
                    ulong colorBits = color.ToULong();

                    int pixelOffset = (y * width + x) * 4;
                    Half* drawPixel = drawArray + pixelOffset;
                    *(ulong*)drawPixel = colorBits;
                }
            }
        });*/
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
