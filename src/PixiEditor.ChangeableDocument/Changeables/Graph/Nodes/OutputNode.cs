using System.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output")]
public class OutputNode : Node, IRenderInput
{
    public const string UniqueName = "PixiEditor.Output";
    public const string InputPropertyName = "Background";
    public const string InputTransformPropertyName = "InputTransform";

    public RenderInputProperty Input { get; }
    public InputProperty<Matrix3X3> InputTransform { get; }

    public OutputNode()
    {
        Input = new RenderInputProperty(this, InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);
        InputTransform = CreateInput(InputTransformPropertyName, "INPUT_TRANSFORM", Matrix3X3.Identity);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
        if (!string.IsNullOrEmpty(context.TargetOutput)) return;

        Input.Value?.Paint(context, context.RenderSurface);

        var previews = context.GetPreviewTexturesForNode(Id);
        if (previews is null) return;
        foreach (var preview in previews)
        {
            if (preview.Texture == null)
                continue;

            int saved = preview.Texture.DrawingSurface.Canvas.Save();
            preview.Texture.DrawingSurface.Canvas.Clear();

            RectD? bounds = new RectD(VecD.Zero, context.DocumentSize);

            VecD scaling = PreviewUtility.CalculateUniformScaling(bounds.Value.Size, preview.Texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(bounds.Value.Size, preview.Texture.Size, scaling);
            RenderContext adjusted =
                PreviewUtility.CreatePreviewContext(context, scaling, bounds.Value.Size, preview.Texture.Size);

            preview.Texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            preview.Texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            preview.Texture.DrawingSurface.Canvas.Translate((float)-bounds.Value.X, (float)-bounds.Value.Y);

            adjusted.RenderSurface = preview.Texture.DrawingSurface.Canvas;
            Input.Value?.Paint(adjusted, adjusted.RenderSurface);
            preview.Texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    RenderInputProperty IRenderInput.Background => Input;
}
