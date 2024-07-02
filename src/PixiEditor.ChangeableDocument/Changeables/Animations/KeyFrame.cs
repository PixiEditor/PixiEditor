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
            TargetLayer.SetKeyFrameLength(Id, startFrame, Duration);
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
            TargetLayer.SetKeyFrameLength(Id, StartFrame, Duration);
        }
    }
    
    public int EndFrame => StartFrame + Duration;
    
    public Guid LayerGuid { get; }
    public Guid Id { get; set; }

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
        }
    }

    public IReadOnlyLayer TargetLayer { get; }

    protected KeyFrame(IReadOnlyLayer layer, int startFrame)
    {
        TargetLayer = layer;
        LayerGuid = layer.GuidValue;
        this.startFrame = startFrame;
        duration = 1;
        Id = Guid.NewGuid();
    }
    
    public abstract KeyFrame Clone();
}
