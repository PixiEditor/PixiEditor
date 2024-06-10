using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public class AnimationData : IReadOnlyAnimationData
{
    private int _currentFrame;

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            int lastFrame = value;
            if (value < 0)
            {
                _currentFrame = 0;
            }
            else
            {
                _currentFrame = value;
            }
            
            OnPreviewFrameChanged(lastFrame);
        }
    }
    
    public List<Clip> Clips { get; set; } = new List<Clip>();
    IReadOnlyList<IReadOnlyClip> IReadOnlyAnimationData.Clips => Clips;
    
    public void ChangePreviewFrame(int frame)
    {
        CurrentFrame = frame;
    }
    
    private void OnPreviewFrameChanged(int lastFrame)
    {
        if (Clips == null)
        {
            return;
        }
        
        foreach (var clip in Clips)
        {
            if (IsWithinRange(clip, CurrentFrame))
            {
                if (!IsWithinRange(clip, lastFrame))
                {
                    clip.Deactivated(CurrentFrame);
                }
                else
                {
                    clip.ActiveFrameChanged(CurrentFrame);   
                }
            }
        }
    }

    private bool IsWithinRange(Clip clip, int frame)
    {
        return frame >= clip.StartFrame && frame < clip.StartFrame + clip.Duration;
    }
}
