using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class KeyFrame : IReadOnlyKeyFrame
{
    private int startFrame;
    private int duration;
    
    public event Action KeyFrameChanged;

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
            KeyFrameChanged?.Invoke();
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
            KeyFrameChanged?.Invoke();
        }
    }
    
    public int EndFrame => StartFrame + Duration;
    
    public Guid LayerGuid { get; }
    public Guid Id { get; set; }

    protected KeyFrame(Guid layerGuid, int startFrame)
    {
        LayerGuid = layerGuid;
        this.startFrame = startFrame;
        duration = 1;
        Id = Guid.NewGuid();
    }

    public virtual void ActiveFrameChanged(int atFrame) { }
    public virtual void Deactivated(int atFrame) { }

    public virtual bool IsWithinRange(int frame)
    {
        return frame >= StartFrame && frame < EndFrame;
    }
}
