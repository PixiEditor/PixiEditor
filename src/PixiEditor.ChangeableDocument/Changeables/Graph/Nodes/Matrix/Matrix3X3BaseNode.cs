using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

public abstract class Matrix3X3BaseNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<Matrix3X3> Input { get; }
    public OutputProperty<Matrix3X3> Matrix { get; }

    private Paint? paint;

    public Matrix3X3BaseNode()
    {
        Background = CreateRenderInput("Background", "IMAGE");
        Input = CreateInput("Input", "INPUT_MATRIX", Matrix3X3.Identity);
        Matrix = CreateOutput("Matrix", "OUTPUT_MATRIX", Matrix3X3.Identity);
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }

    protected override void OnExecute(RenderContext context)
    {
        Matrix.Value = CalculateMatrix(Input.Value);
        if (Background.Value == null)
            return;

        paint ??= new();
        base.OnExecute(context);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (paint == null)
            return;

        int layer = surface.Canvas.Save();

        surface.Canvas.SetMatrix(surface.Canvas.TotalMatrix.Concat(Matrix.Value));
        Background.Value?.Paint(context, surface);

        surface.Canvas.RestoreToCount(layer);
    }

    protected abstract Matrix3X3 CalculateMatrix(Matrix3X3 input);
}
