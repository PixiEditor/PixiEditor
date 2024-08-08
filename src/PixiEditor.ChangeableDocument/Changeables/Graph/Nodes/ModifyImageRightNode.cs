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
public class ModifyImageRightNode : Node, IPairNodeEnd
{
    public Node StartNode { get; set; }

    private Paint drawingPaint = new Paint() { BlendMode = BlendMode.Src };

    public FuncInputProperty<Float2> Coordinate { get; }
    public FuncInputProperty<Half4> Color { get; }

    public OutputProperty<Texture> Output { get; }

    public override string DisplayName { get; set; } = "MODIFY_IMAGE_RIGHT_NODE";

    private string _lastSksl;
    
    private TextureCache _textureCache = new(); 

    public ModifyImageRightNode()
    {
        Coordinate = CreateFuncInput(nameof(Coordinate), "UV", new Float2("coords"));
        Color = CreateFuncInput(nameof(Color), "COLOR", new Half4(""));
        Output = CreateOutput<Texture>(nameof(Output), "OUTPUT", null);
    }

    protected override Texture? OnExecute(RenderingContext renderingContext)
    {
        if (StartNode == null)
        {
            FindStartNode();
            if (StartNode == null)
            {
                return null;
            }
        }

        var startNode = StartNode as ModifyImageLeftNode;
        if (startNode.Image.Value is not { Size: var size })
        {
            return null;
        }

        var width = size.X;
        var height = size.Y;

        Texture surface = _textureCache.GetTexture(renderingContext.ChunkResolution, size); 

        if (!surface.IsHardwareAccelerated)
        {
            startNode.PreparePixmap(renderingContext);

            using Pixmap targetPixmap = surface.PeekReadOnlyPixels();

            ModifyImageInParallel(renderingContext, targetPixmap, width, height);

            startNode.DisposePixmap(renderingContext);
        }
        else
        {
            ShaderBuilder builder = new();
            FuncContext context = new(renderingContext, builder);

            if (Coordinate.Connection != null)
            {
                var coordinate = Coordinate.Value(context);
                if (string.IsNullOrEmpty(coordinate.UniformName))
                {
                    builder.SetConstant(context.Position, coordinate);
                }
                else
                {
                    builder.Set(context.Position, coordinate);
                }
            }
            else
            {
                var constCoords = Coordinate.NonOverridenValue(FuncContext.NoContext);
                constCoords.UniformName = "constCords";
                builder.AddUniform(constCoords.UniformName, constCoords.ConstantValue);
                builder.Set(context.Position, constCoords);
            }

            if (Color.Connection != null)
            {
                builder.ReturnVar(Color.Value(context));
            }
            else
            {
                Half4 color = Color.NonOverridenValue(FuncContext.NoContext);
                color.UniformName = "color";
                builder.AddUniform(color.UniformName, color.ConstantValue);
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

        return Output.Value;
    }

    private unsafe void ModifyImageInParallel(RenderingContext renderingContext, Pixmap targetPixmap, int width,
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
        _textureCache.Dispose(); 
        drawingPaint?.Dispose();
    }

    private void FindStartNode()
    {
        TraverseBackwards(node =>
        {
            if (node is ModifyImageLeftNode leftNode)
            {
                StartNode = leftNode;
                return false;
            }

            return true;
        });
    }

    public override Node CreateCopy() => new ModifyImageRightNode();
}
