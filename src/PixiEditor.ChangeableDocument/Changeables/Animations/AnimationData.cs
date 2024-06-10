using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public class AnimationData : IReadOnlyAnimationData
{
    private int _activeFrame;

    public int ActiveFrame
    {
        get => _activeFrame;
        set
        {
            int lastFrame = value;
            if (value < 0)
            {
                _activeFrame = 0;
            }
            else
            {
                _activeFrame = value;
            }
            
            OnPreviewFrameChanged(lastFrame);
        }
    }
    
    public List<KeyFrame> KeyFrames { get; set; } = new List<KeyFrame>();
    IReadOnlyList<IReadOnlyKeyFrame> IReadOnlyAnimationData.KeyFrames => KeyFrames;
    
    public void ChangePreviewFrame(int frame)
    {
        ActiveFrame = frame;
    }
    
    private void OnPreviewFrameChanged(int lastFrame)
    {
        if (KeyFrames == null)
        {
            return;
        }
        
        foreach (var keyFrame in KeyFrames)
        {
            if (IsWithinRange(keyFrame, ActiveFrame))
            {
                if (!IsWithinRange(keyFrame, lastFrame))
                {
                    keyFrame.Deactivated(ActiveFrame);
                }
                else
                {
                    keyFrame.ActiveFrameChanged(ActiveFrame);   
                }
            }
        }
    }

    private bool IsWithinRange(KeyFrame keyFrame, int frame)
    {
        return frame >= keyFrame.StartFrame && frame < keyFrame.StartFrame + keyFrame.Duration;
    }
}
