using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;

public class TimeNode : Node
{
    protected override string NodeUniqueName { get; } = "Time";
    public override string DisplayName { get; set; } = "TIME_NODE";
    
    public OutputProperty<int> ActiveFrame { get; set; }
    public OutputProperty<double> NormalizedTime { get; set; }

    protected override bool AffectedByAnimation => true;

    public TimeNode()
    {
        ActiveFrame = CreateOutput("ActiveFrame", "ACTIVE_FRAME", 0);
        NormalizedTime = CreateOutput("NormalizedTime", "NORMALIZED_TIME", 0.0);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        ActiveFrame.Value = context.FrameTime.Frame;
        NormalizedTime.Value = context.FrameTime.NormalizedTime;
        
        return null;
    }

    public override bool AreInputsLegal()
    {
        return true;
    }

    public override Node CreateCopy()
    {
        return new TimeNode();
    }
}
