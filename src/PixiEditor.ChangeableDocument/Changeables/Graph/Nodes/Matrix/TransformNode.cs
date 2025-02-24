using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Transform")]
public class TransformNode : RenderNode, IRenderInput
{
    public RenderInputProperty Background { get; }
    public InputProperty<Matrix3X3> Matrix { get; }

    public TransformNode()
    {
        Background = CreateRenderInput("Background", "IMAGE");
        Matrix = CreateInput("Matrix", "INPUT", Matrix3X3.Identity);

        Output.FirstInChain = null;
    }

    protected override void OnPaint(RenderContext context, DrawingSurface surface)
    {
        if (Background.Value == null)
            return;

        int layer = surface.Canvas.Save();

        surface.Canvas.SetMatrix(surface.Canvas.TotalMatrix.PostConcat(Matrix.Value));
        Background.Value?.Paint(context, surface);

        surface.Canvas.RestoreToCount(layer);
    }

    public override RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        return null;
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        return false;
    }

    public override Node CreateCopy()
    {
        return new TransformNode();
    }
}
