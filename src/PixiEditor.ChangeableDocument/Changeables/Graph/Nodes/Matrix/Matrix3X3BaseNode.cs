using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

public abstract class Matrix3X3BaseNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public FuncInputProperty<Float3x3> Input { get; }
    public InputProperty<ShapeVectorData> InputVector { get; }
    public FuncOutputProperty<Float3x3> Matrix { get; }
    public OutputProperty<ShapeVectorData> OutputVector { get; }

    public Matrix3X3BaseNode()
    {
        Background = CreateRenderInput("Background", "IMAGE");
        Input = CreateFuncInput<Float3x3>("Input", "INPUT_MATRIX",
            new Float3x3("") { ConstantValue = Matrix3X3.Identity });
        InputVector = CreateInput<ShapeVectorData>("VectorShape", "SHAPE_LABEL", null);
        Matrix = CreateFuncOutput<Float3x3>("Matrix", "OUTPUT_MATRIX",
            (c) => CalculateMatrix(c, c.GetValue(Input)));

        OutputVector = CreateOutput<ShapeVectorData>("OutputVector", "VECTOR", null);
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }

    protected override void OnExecute(RenderContext context)
    {
        if (InputVector.Value != null)
        {
            var clone = InputVector.Value.Clone() as ShapeVectorData;
            Matrix3X3 mtx = Matrix.Value.Invoke(FuncContext.NoContext).GetConstant() as Matrix3X3? ??
                            Matrix3X3.Identity;

            clone.TransformationMatrix = clone.TransformationMatrix.PostConcat(mtx);
            OutputVector.Value = clone;
        }

        if (Background.Value == null)
            return;

        base.OnExecute(context);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        int layer = surface.Canvas.Save();

        Float3x3 mtx = Matrix.Value.Invoke(FuncContext.NoContext);

        Matrix3X3 constant = mtx.GetConstant() as Matrix3X3? ?? Matrix3X3.Identity;
        surface.Canvas.SetMatrix(
            surface.Canvas.TotalMatrix.Concat(constant));

        var clonedCtx = context.Clone();
        if (clonedCtx.VisibleDocumentRegion.HasValue)
        {
            clonedCtx.VisibleDocumentRegion =
                (RectI)constant.Invert().TransformRect((RectD)clonedCtx.VisibleDocumentRegion.Value);
        }

        if (!surface.LocalClipBounds.IsZeroOrNegativeArea)
        {
            Background.Value?.Paint(clonedCtx, surface);
        }

        surface.Canvas.RestoreToCount(layer);
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName = "")
    {
        if (Background.Value == null)
            return null;

        return base.GetPreviewBounds(ctx, elementToRenderName);
    }

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        return Background.Value != null;
    }

    protected abstract Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input);
}
