using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class KeyFrame : IReadOnlyKeyFrame, IDisposable
{
    private int startFrame;
    private int duration;
    private bool isVisible = true;
    
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

    public bool IsVisible
    {
        get => isVisible;
        set
        {
            isVisible = value;
            OnVisibilityChanged();
            KeyFrameChanged?.Invoke();
        }
    }

    protected KeyFrame(Guid layerGuid, int startFrame)
    {
        LayerGuid = layerGuid;
        this.startFrame = startFrame;
        duration = 1;
        Id = Guid.NewGuid();
    }
    
    public virtual bool IsWithinRange(int frame)
    {
        return frame >= StartFrame && frame < EndFrame;
    }

    public abstract KeyFrame Clone();

    public virtual void Dispose() { }
    
    protected virtual void OnVisibilityChanged() { }
}
