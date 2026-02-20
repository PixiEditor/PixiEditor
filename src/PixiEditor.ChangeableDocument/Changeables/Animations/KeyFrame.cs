using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public delegate void KeyFrameVisibilityChangedHandler(Guid keyFrameId, bool isVisible);
public delegate void KeyFrameLengthChangedHandler(Guid keyFrameId, int startFrame, int duration);
public abstract class KeyFrame : IReadOnlyKeyFrame
{
    private int startFrame;
    private int duration;
    private bool isVisible = true;
    
    public event KeyFrameVisibilityChangedHandler? KeyFrameVisibilityChanged;
    public event KeyFrameLengthChangedHandler? KeyFrameLengthChanged;
    
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
            KeyFrameLengthChanged?.Invoke(Id, StartFrame, Duration);
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
            KeyFrameLengthChanged?.Invoke(Id, StartFrame, Duration);
        }
    }
    
    public int EndFrame => StartFrame + Duration - 1;
    
    public Guid NodeId { get; }
    public Guid Id { get; set; }

    public virtual bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            KeyFrameVisibilityChanged?.Invoke(Id, IsVisible);
        }
    }

    protected KeyFrame(Guid targetNode, int startFrame)
    {
        NodeId = targetNode;
        this.startFrame = startFrame;
        duration = 1;
        Id = Guid.NewGuid();
    }
    
    public abstract KeyFrame Clone();

    public void ClearEvents()
    {
        KeyFrameVisibilityChanged = null;
        KeyFrameLengthChanged = null;
    }
}
