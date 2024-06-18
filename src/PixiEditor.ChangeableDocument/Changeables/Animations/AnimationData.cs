using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class AnimationData : IReadOnlyAnimationData
{
    private int _activeFrame;

    public int ActiveFrame
    {
        get => _activeFrame;
        set
        {
            _activeFrame = value < 0 ? 0 : value;

            OnPreviewFrameChanged();
        }
    }

    public IReadOnlyList<IReadOnlyKeyFrame> KeyFrames => keyFrames;

    private List<KeyFrame> keyFrames = new List<KeyFrame>();
    private readonly Document document;
    private List<KeyFrame> lastActiveKeyFrames = new List<KeyFrame>();
    
    public AnimationData(Document document)
    {
        this.document = document;
    }

    public void AddKeyFrame(KeyFrame keyFrame)
    {
        Guid id = keyFrame.LayerGuid;
        if (TryFindKeyFrameCallback(id, out GroupKeyFrame group))
        {
            group.Children.Add(keyFrame);
        }
        else
        {
            GroupKeyFrame createdGroup = new GroupKeyFrame(id, keyFrame.StartFrame, document);
            createdGroup.Children.Add(keyFrame);
            keyFrames.Add(createdGroup);
        }
        
        keyFrame.KeyFrameChanged += KeyFrameChanged;
        
        UpdateKeyFrames(keyFrames);
    }

    public void RemoveKeyFrame(Guid createdKeyFrameId)
    {
        TryFindKeyFrameCallback<KeyFrame>(createdKeyFrameId, out _, (frame, parent) =>
        {
            frame.KeyFrameChanged -= KeyFrameChanged;
            parent?.Children.Remove(frame);
        });
        
        UpdateKeyFrames(keyFrames);
    }

    public bool TryFindKeyFrame<T>(Guid id, out T keyFrame) where T : IReadOnlyKeyFrame
    {
        return TryFindKeyFrameCallback(id, out keyFrame, null);
    }
    
    private void KeyFrameChanged()
    {
        UpdateKeyFrames(keyFrames);
    }

    private bool TryFindKeyFrameCallback<T>(Guid id, out T? foundKeyFrame,
        Action<KeyFrame, GroupKeyFrame?> onFound = null) where T : IReadOnlyKeyFrame
    {
        return TryFindKeyFrame(keyFrames, null, id, out foundKeyFrame, onFound);
    }

    private bool TryFindKeyFrame<T>(List<KeyFrame> root, GroupKeyFrame parent, Guid id, out T? result,
        Action<KeyFrame, GroupKeyFrame?> onFound) where T : IReadOnlyKeyFrame
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

    private void OnPreviewFrameChanged()
    {
        if (KeyFrames == null)
        {
            return;
        }

        UpdateKeyFrames(keyFrames);
    }

    private void UpdateKeyFrames(List<KeyFrame> root)
    {
        foreach (var keyFrame in root)
        {
            if (!keyFrame.IsVisible)
            {
                if (lastActiveKeyFrames.Contains(keyFrame))
                {
                    keyFrame.Deactivated(ActiveFrame);
                    lastActiveKeyFrames.Remove(keyFrame);
                }
            }
            else
            {
                bool isWithinRange = keyFrame.IsWithinRange(ActiveFrame);
                if (lastActiveKeyFrames.Contains(keyFrame))
                {
                    if (!isWithinRange)
                    {
                        keyFrame.Deactivated(ActiveFrame);
                        lastActiveKeyFrames.Remove(keyFrame);
                    }
                    else
                    {
                        keyFrame.ActiveFrameChanged(ActiveFrame);
                    }
                }
                else if (isWithinRange)
                {
                    keyFrame.ActiveFrameChanged(ActiveFrame);
                    lastActiveKeyFrames.Add(keyFrame);
                }
            }

            if (keyFrame is GroupKeyFrame group)
            {
                UpdateKeyFrames(group.Children);
            }
        }
    }
}
