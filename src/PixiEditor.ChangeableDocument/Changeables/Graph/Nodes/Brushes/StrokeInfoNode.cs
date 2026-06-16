using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

[NodeInfo("StrokeInfo")]
public class StrokeInfoNode : Node, IBrushSampleTextureNode
{
    public OutputProperty<float> StrokeWidth { get; }
    public OutputProperty<VecI> ComputedSampleSize { get; }
    public OutputProperty<VecD> StartPoint { get; }
    public OutputProperty<VecD> LastAppliedPoint { get; }
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<VecD> TargetSampleTexturePos { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }

    public StrokeInfoNode()
    {
        StrokeWidth = CreateOutput<float>("StrokeWidth", "STROKE_WIDTH", 1f);
        ComputedSampleSize = CreateOutput<VecI>("ComputedSampleSize", "COMPUTED_SAMPLE_SIZE", VecI.Zero);
        StartPoint = CreateOutput<VecD>("StartPoint", "START_POINT", VecD.Zero);
        LastAppliedPoint = CreateOutput<VecD>("LastAppliedPoint", "LAST_APPLIED_POINT", VecD.Zero);
        TargetSampleTexture = CreateOutput<Texture>("TargetSampleTexture", "TARGET_SAMPLE_TEXTURE", null);
        TargetSampleTexturePos = CreateOutput<VecD>("TargetSampleTexturePos", "TARGET_SAMPLE_TEXTURE_POS", VecD.Zero);
        TargetFullTexture = CreateOutput<Texture>("TargetFullTexture", "TARGET_FULL_TEXTURE", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context is not BrushRenderContext brushRenderContext)
            return;

        StrokeWidth.Value = brushRenderContext.BrushData.StrokeWidth;
        StartPoint.Value = brushRenderContext.StartPoint;
        LastAppliedPoint.Value = brushRenderContext.LastAppliedPoint;
        TargetSampleTexturePos.Value = brushRenderContext.TargetSampleTexturePos;

        if (TargetSampleTexture.Connections.Count > 0)
        {
            TargetSampleTexture.Value = brushRenderContext.TargetSampledTexture;
            ComputedSampleSize.Value = brushRenderContext.TargetSampledTexture?.Size ?? VecI.Zero;
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
