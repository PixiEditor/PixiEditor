using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

public abstract class Matrix3X3BaseNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public FuncInputProperty<Float3x3, ShaderFuncContext> Input { get; }
    public FuncOutputProperty<Float3x3, ShaderFuncContext> Matrix { get; }

    public Matrix3X3BaseNode()
    {
        Background = CreateRenderInput("Background", "IMAGE");
        Input = CreateFuncInput<Float3x3, ShaderFuncContext>("Input", "INPUT_MATRIX",
            new Float3x3("") { ConstantValue = Matrix3X3.Identity });
        Matrix = CreateFuncOutput<Float3x3, ShaderFuncContext>("Matrix", "OUTPUT_MATRIX",
            (c) => CalculateMatrix(c, c.GetValue(Input)));
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Background.Value == null)
            return;

        base.OnExecute(context);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        int layer = surface.Canvas.Save();

        Float3x3 mtx = Matrix.Value.Invoke(ShaderFuncContext.NoContext);

        surface.Canvas.SetMatrix(
            surface.Canvas.TotalMatrix.Concat(mtx.GetConstant() as Matrix3X3? ?? Matrix3X3.Identity));
        if (!surface.LocalClipBounds.IsZeroOrNegativeArea)
        {
            Background.Value?.Paint(context, surface);
        }

        surface.Canvas.RestoreToCount(layer);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (Background.Value == null)
            return null;

        return base.GetPreviewBounds(frame, elementToRenderName);
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Background.Value == null)
            return false;

        return base.RenderPreview(renderOn, context, elementToRenderName);
    }

    protected abstract Float3x3 CalculateMatrix(ShaderFuncContext ctx, Float3x3 input);
}
