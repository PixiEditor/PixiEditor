using System.Collections;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

internal class AnimationData : IReadOnlyAnimationData
{
    public int FrameRate { get; set; } = 60;
    public int OnionFrames { get; set; } = 1;
    public int DefaultEndFrame { get; set; } = 60;
    public IReadOnlyList<IReadOnlyKeyFrame> KeyFrames => keyFrames;
    public double OnionOpacity { get; set; } = 50;
    public bool FallbackAnimationToLayerImage { get; set; }

    private List<KeyFrame> keyFrames = new List<KeyFrame>();
    private readonly Document document;

    public AnimationData(Document document)
    {
        this.document = document;
    }

    public void AddKeyFrame(KeyFrame keyFrame)
    {
        Guid id = keyFrame.NodeId;
        if (TryFindKeyFrameCallback(id, out GroupKeyFrame group))
        {
            group.Children.Add(keyFrame);
        }
        else if (keyFrame is GroupKeyFrame groupKeyFrame)
        {
            keyFrames.Add(groupKeyFrame);
            foreach (var child in groupKeyFrame.Children)
            {
                SubscribeToKeyFrameEvents(child);
            }
        }
        else
        {
            var node = document.FindNodeOrThrow<Node>(id);
            GroupKeyFrame createdGroup = new GroupKeyFrame(node.Id, keyFrame.StartFrame, document);
            createdGroup.Children.Add(keyFrame);
            keyFrames.Add(createdGroup);
        }

        SubscribeToKeyFrameEvents(keyFrame);
    }

    private void SubscribeToKeyFrameEvents(KeyFrame keyFrame)
    {
        Node node = document.FindNodeOrThrow<Node>(keyFrame.NodeId);

        keyFrame.KeyFrameVisibilityChanged += node.SetKeyFrameVisibility;
        keyFrame.KeyFrameLengthChanged += node.SetKeyFrameLength;
    }
    
    private void UnsubscribeFromKeyFrameEvents(KeyFrame keyFrame)
    {
        keyFrame.ClearEvents();
    }

    public void RemoveKeyFrame(Guid createdKeyFrameId)
    {
        TryFindKeyFrameCallback<KeyFrame>(createdKeyFrameId, out _, (frame, parent) =>
        {
            if (frame is GroupKeyFrame group)
            {
                keyFrames.Remove(group);
                foreach (var child in group.Children)
                {
                    RemoveKeyFrame(child.Id);
                }
            }

            if (document.TryFindNode<Node>(frame.NodeId, out Node? node))
            {
                node.RemoveKeyFrame(frame.Id);
            }

            parent?.Children.Remove(frame);
            
            if (parent?.Children.Count == 0)
            {
                keyFrames.Remove(parent);
            }
            
            UnsubscribeFromKeyFrameEvents(frame);
        });
    }

    public bool TryFindKeyFrame<T>(Guid id, out T keyFrame) where T : IReadOnlyKeyFrame
    {
        return TryFindKeyFrameCallback(id, out keyFrame, null);
    }

    public IReadOnlyAnimationData Clone()
    {
        AnimationData clone = new AnimationData(document)
        {
            FrameRate = FrameRate,
            OnionFrames = OnionFrames,
            DefaultEndFrame = DefaultEndFrame,
            OnionOpacity = OnionOpacity
        };
        foreach (var keyFrame in keyFrames)
        {
            clone.keyFrames.Add(keyFrame.Clone());
        }

        return clone;
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

    public KeyFrame? TryGetKeyFrameAtFrame(Guid? groupId, int frame)
    {
        if (groupId.HasValue)
        {
            if (TryFindKeyFrameCallback(groupId.Value, out GroupKeyFrame group))
            {
                return group.Children.FirstOrDefault(kf => kf.StartFrame <= frame && kf.StartFrame + kf.Duration > frame);
            }
        }
        else
        {
            return keyFrames.FirstOrDefault(kf => kf.StartFrame <= frame && kf.StartFrame + kf.Duration > frame);
        }

        return null;
    }

    public KeyFrame TryGetNextKeyFrameAtFrame(Guid groupId, int frame, Guid[]? toIgnore = null)
    {
        if (TryFindKeyFrameCallback(groupId, out GroupKeyFrame group))
        {
            return group.Children.OrderBy(kf => kf.StartFrame).FirstOrDefault(kf => kf.StartFrame > frame && (toIgnore == null || !toIgnore.Contains(kf.Id)));
        }

        return keyFrames.OrderBy(kf => kf.StartFrame).FirstOrDefault(kf => kf.StartFrame > frame && (toIgnore == null || !toIgnore.Contains(kf.Id)));
    }

    public KeyFrame TryGetPreviousKeyFrameAtFrame(Guid groupId, int frame, Guid[]? toIgnore = null)
    {
        if (TryFindKeyFrameCallback(groupId, out GroupKeyFrame group))
        {
            return group.Children.OrderByDescending(kf => kf.StartFrame).FirstOrDefault(kf => kf.StartFrame < frame && (toIgnore == null || !toIgnore.Contains(kf.Id)));
        }

        return keyFrames.OrderByDescending(kf => kf.StartFrame).FirstOrDefault(kf => kf.StartFrame < frame && (toIgnore == null || !toIgnore.Contains(kf.Id)));
    }

    public List<KeyFrame> GetKeyFramesForNode(Guid groupId)
    {
        if (TryFindKeyFrameCallback(groupId, out GroupKeyFrame group))
        {
            return group.Children;
        }

        return [];
    }

    public List<KeyFrame> GetPreviousKeyFramesForNode(Guid groupId, Guid targetKeyFrame, int untilInclusive)
    {
        var target = TryFindKeyFrameCallback(groupId, out GroupKeyFrame group) ? group.Children.FirstOrDefault(kf => kf.Id == targetKeyFrame) : keyFrames.FirstOrDefault(kf => kf.Id == targetKeyFrame);
        if (target == null)
        {
            return [];
        }

        var previous = group.Children.Where(kf => kf.StartFrame < target.StartFrame && kf.EndFrame >= untilInclusive).ToList();
        return previous;
    }

    public List<KeyFrame> GetNextKeyFramesForNode(Guid groupId, Guid targetKeyFrame, int untilInclusive)
    {
        var target = TryFindKeyFrameCallback(groupId, out GroupKeyFrame group) ? group.Children.FirstOrDefault(kf => kf.Id == targetKeyFrame) : keyFrames.FirstOrDefault(kf => kf.Id == targetKeyFrame);
        if (target == null)
        {
            return [];
        }

        var next = group.Children.Where(kf => kf.EndFrame > target.EndFrame && kf.StartFrame <= untilInclusive).ToList();
        return next;
    }
}
