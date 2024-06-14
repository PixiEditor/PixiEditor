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

    public IReadOnlyList<IReadOnlyKeyFrame> KeyFrames => keyFrames;

    private List<KeyFrame> keyFrames = new List<KeyFrame>();
    
    public void AddKeyFrame(KeyFrame keyFrame)
    {
        Guid id = keyFrame.LayerGuid;
        if (TryFindKeyFrame(id, out GroupKeyFrame group))
        {
            group.Children.Add(keyFrame);
        }
        else
        {
            GroupKeyFrame createdGroup = new GroupKeyFrame(id, keyFrame.StartFrame);
            createdGroup.Children.Add(keyFrame);
            keyFrames.Add(createdGroup);
        }
    }

    public void RemoveKeyFrame(Guid createdKeyFrameId)
    {
        TryFindKeyFrame<KeyFrame>(createdKeyFrameId, out _, (frame, parent) =>
        {
            if (parent != null)
            {
                parent.Children.Remove(frame);
            }
        });
    }
    
    public bool FindKeyFrame(Guid id, out IReadOnlyKeyFrame keyFrame)
    {
        return TryFindKeyFrame(id, out keyFrame, null);
    }

    private bool TryFindKeyFrame<T>(Guid id, out T? foundKeyFrame, Action<KeyFrame, GroupKeyFrame?> onFound = null) where T : IReadOnlyKeyFrame
    {
        return TryFindKeyFrame(keyFrames, null, id, out foundKeyFrame, onFound);
    }

    private bool TryFindKeyFrame<T>(List<KeyFrame> root, GroupKeyFrame parent, Guid id, out T? result, Action<KeyFrame, GroupKeyFrame?> onFound) where T : IReadOnlyKeyFrame
    {
        for (var i = 0; i < root.Count; i++)
        {
            var frame = root[i];
            if (frame is T targetFrame && targetFrame.Id.Equals(id))
            {
                result = targetFrame;
                onFound?.Invoke(frame, parent);
                return true;
            }

            if (frame is GroupKeyFrame { Children.Count: > 0 } group)
            {
                bool found = TryFindKeyFrame(group.Children, group, id, out result, onFound);
                if (found)
                {
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
    
    private void OnPreviewFrameChanged(int lastFrame)
    {
        if (KeyFrames == null)
        {
            return;
        }

        NotifyKeyFrames(lastFrame, keyFrames);
    }

    private void NotifyKeyFrames(int lastFrame, List<KeyFrame> root)
    {
        foreach (var keyFrame in root)
        {
            if (keyFrame is GroupKeyFrame group)
            {
                NotifyKeyFrames(lastFrame, group.Children);
            }
            else
            {
                if (IsWithinRange(keyFrame, ActiveFrame))
                {
                    if (!IsWithinRange(keyFrame, lastFrame))
                    {
                        keyFrame.Deactivated(ActiveFrame);
                    }
                    else
                    {
                        Console.WriteLine($"{ActiveFrame}");
                        keyFrame.ActiveFrameChanged(ActiveFrame);
                    }
                }
            }
        }
    }

    private bool IsWithinRange(KeyFrame keyFrame, int frame)
    {
        return frame >= keyFrame.StartFrame && frame < keyFrame.StartFrame + keyFrame.Duration;
    }
}
