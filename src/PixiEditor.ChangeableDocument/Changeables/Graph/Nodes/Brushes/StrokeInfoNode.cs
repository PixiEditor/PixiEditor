using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("StrokeInfo")]
public class StrokeInfoNode : Node, IBrushSampleTextureNode
{
    public OutputProperty<float> StrokeWidth { get; }
    public OutputProperty<float> Spacing { get; }
    public OutputProperty<VecD> StartPoint { get; }
    public OutputProperty<VecD> LastAppliedPoint { get; }
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }

    public StrokeInfoNode()
    {
        StrokeWidth = CreateOutput<float>("StrokeWidth", "STROKE_WIDTH", 1f);
        Spacing = CreateOutput<float>("Spacing", "SPACING", 0.1f);
        StartPoint = CreateOutput<VecD>("StartPoint", "START_POINT", VecD.Zero);
        LastAppliedPoint = CreateOutput<VecD>("LastAppliedPoint", "LAST_APPLIED_POINT", VecD.Zero);
        TargetSampleTexture = CreateOutput<Texture>("TargetSampleTexture", "TARGET_SAMPLE_TEXTURE", null);
        TargetFullTexture = CreateOutput<Texture>("TargetFullTexture", "TARGET_FULL_TEXTURE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context is not BrushRenderContext brushRenderContext)
            return;

        StrokeWidth.Value = brushRenderContext.BrushData.StrokeWidth;
        Spacing.Value = brushRenderContext.BrushData.Spacing;
        StartPoint.Value = brushRenderContext.StartPoint;
        LastAppliedPoint.Value = brushRenderContext.LastAppliedPoint;

        if (TargetSampleTexture.Connections.Count > 0)
        {
            TargetSampleTexture.Value = brushRenderContext.TargetSampledTexture;
        }

        if (TargetFullTexture.Connections.Count > 0)
        {
            TargetFullTexture.Value = brushRenderContext.TargetFullTexture;
        }
    }

    public override Node CreateCopy()
    {
        return new StrokeInfoNode();
    }
}
