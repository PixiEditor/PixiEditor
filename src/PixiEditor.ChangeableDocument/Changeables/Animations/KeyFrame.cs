using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class KeyFrame : IReadOnlyKeyFrame
{
    private int startFrame;
    private int duration;
    private bool isVisible = true;
    
    public virtual int StartFrame
    {
        get => startFrame;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            startFrame = value;
            TargetNode.SetKeyFrameLength(Id, startFrame, Duration);
        }
    }

    public virtual int Duration
    {
        get => duration;
        set
        {
            if (value < 1)
            {
                value = 1;
            }

            duration = value;
            TargetNode.SetKeyFrameLength(Id, StartFrame, Duration);
        }
    }
    
    public int EndFrame => StartFrame + Duration;
    
    public Guid NodeId { get; }
    public Guid Id { get; set; }

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
        }
    }

    public Node TargetNode { get; }
    
    IReadOnlyNode IReadOnlyKeyFrame.TargetNode => TargetNode;

    protected KeyFrame(Node node, int startFrame)
    {
        TargetNode = node;
        NodeId = node.Id;
        this.startFrame = startFrame;
        duration = 1;
        Id = Guid.NewGuid();
    }
    
    public abstract KeyFrame Clone();
}
