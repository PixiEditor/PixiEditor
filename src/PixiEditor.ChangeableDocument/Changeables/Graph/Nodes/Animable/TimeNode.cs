using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;

[NodeInfo("Time")]
public class TimeNode : Node
{
    public OutputProperty<int> ActiveFrame { get; set; }
    public OutputProperty<double> NormalizedTime { get; set; }

    protected override bool AffectedByAnimation => true;

    public TimeNode()
    {
        ActiveFrame = CreateOutput("ActiveFrame", "ACTIVE_FRAME", 0);
        NormalizedTime = CreateOutput("NormalizedTime", "NORMALIZED_TIME", 0.0);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        ActiveFrame.Value = context.FrameTime.Frame;
        NormalizedTime.Value = context.FrameTime.NormalizedTime;
        
        return null;
    }

    public override Node CreateCopy()
    {
        return new TimeNode();
    }
}
