using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class KeyFramesStartPos_UpdateableChange : InterruptableUpdateableChange
{
    public Guid[] KeyFramesGuid { get; }
    public int Delta { get; set; }

    private Dictionary<Guid, int> originalStartFrames = new();
    private int relativeDelta;

    private List<Guid> sortedKeyFrames;

    [GenerateUpdateableChangeActions]
    public
        KeyFramesStartPos_UpdateableChange(List<Guid> keyFramesGuid,
            int delta) // do not delete, code generator uses this for update method
    {
        KeyFramesGuid = keyFramesGuid.ToArray();
        Delta = 0;
    }

    [UpdateChangeMethod]
    public void Update(int delta)
    {
        Delta += delta;
        relativeDelta = delta;
    }

    public override bool InitializeAndValidate(Document target)
    {
        List<KeyFrame> keyFramesInOrder = new List<KeyFrame>();
        foreach (Guid keyFrameGuid in KeyFramesGuid)
        {
            if (target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame))
            {
                var node = target.FindNode<Node>(keyFrame.NodeId);
                if (node is null)
                {
                    return false;
                }

                if (node.KeyFrames.FirstOrDefault()?.KeyFrameGuid == keyFrameGuid)
                {
                    return false;
                }

                originalStartFrames[keyFrameGuid] = keyFrame.StartFrame;
                keyFramesInOrder.Add(keyFrame);
            }
            else
            {
                return false;
            }
        }

        sortedKeyFrames = keyFramesInOrder.OrderBy(kf => kf.StartFrame).Select(x => x.Id).ToList();
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        int iterations = Math.Abs(relativeDelta);
        int delta = Math.Sign(relativeDelta);

        bool backwards = relativeDelta < 0;
        var keyFramesInOrder = !backwards ? sortedKeyFrames.AsEnumerable().Reverse() : sortedKeyFrames;
        foreach (var guid in keyFramesInOrder)
        {
            for (int i = 0; i < iterations; i++)
            {
                ApplyOnce(target, delta, guid);
            }
        }

        List<IChangeInfo> changeInfos = new();
        foreach (var keyFrame in originalStartFrames)
        {
            target.AnimationData.TryFindKeyFrame(keyFrame.Key, out KeyFrame kf);
            changeInfos.Add(new KeyFrameLength_ChangeInfo(keyFrame.Key, kf.StartFrame, kf.Duration));
        }

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        List<IChangeInfo> changes = new();
        ResetKeyFramesToOriginal(target);

        bool backwards = Delta < 0;
        var keyFramesInOrder = !backwards ? sortedKeyFrames.AsEnumerable().Reverse() : sortedKeyFrames;
        foreach (var guid in keyFramesInOrder)
        {
            for (int i = 0; i < Math.Abs(Delta); i++)
            {
                int delta = Math.Sign(Delta);
                ApplyOnce(target, delta, guid);
            }
        }

        foreach (var keyFrame in originalStartFrames)
        {
            target.AnimationData.TryFindKeyFrame(keyFrame.Key, out KeyFrame kf);
            changes.Add(new KeyFrameLength_ChangeInfo(keyFrame.Key, kf.StartFrame, kf.Duration));
        }

        ignoreInUndo = false;
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changes = new();

        ResetKeyFramesToOriginal(target);

        foreach (var keyFrame in originalStartFrames)
        {
            target.AnimationData.TryFindKeyFrame(keyFrame.Key, out KeyFrame kf);
            changes.Add(new KeyFrameLength_ChangeInfo(keyFrame.Key, kf.StartFrame, kf.Duration));
        }

        return changes;
    }

    private void ApplyOnce(Document target, int delta, Guid keyFrameGuid)
    {
        target.AnimationData.TryFindKeyFrame(keyFrameGuid, out KeyFrame keyFrame);
        int rangeStart = Math.Min(keyFrame.StartFrame, keyFrame.StartFrame + delta);
        int rangeEnd = Math.Max(keyFrame.EndFrame, keyFrame.EndFrame + delta);
        int finalDelta = AdjustNeighbors(target, keyFrame, rangeStart, rangeEnd, delta);
        int newStartPos = Math.Max(keyFrame.StartFrame + finalDelta, 1);
        keyFrame.StartFrame = newStartPos;
    }

    private void ResetKeyFramesToOriginal(Document target)
    {
        foreach (var original in originalStartFrames)
        {
            target.AnimationData.TryFindKeyFrame(original.Key, out KeyFrame keyFrame);
            keyFrame.StartFrame = original.Value;
        }
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is KeyFramesStartPos_UpdateableChange otherChange &&
               otherChange.KeyFramesGuid.SequenceEqual(KeyFramesGuid);
    }

    private int AdjustNeighbors(Document target, KeyFrame keyFrame, int rangeStart, int rangeEnd, int delta)
    {
        bool backwards = delta < 0;
        var keyFramesBetweenOldAndNew = backwards
            ? target.AnimationData.GetPreviousKeyFramesForNode(keyFrame.NodeId, keyFrame.Id, rangeStart)
            : target.AnimationData.GetNextKeyFramesForNode(keyFrame.NodeId, keyFrame.Id, rangeEnd);
        int finalDelta = delta;
        foreach (var neighbor in keyFramesBetweenOldAndNew)
        {
            if (KeyFramesGuid.Contains(neighbor.Id))
                continue;

            if (!originalStartFrames.ContainsKey(neighbor.Id))
                originalStartFrames[neighbor.Id] = neighbor.StartFrame;

            int originalKeyFrameStart = neighbor.StartFrame;
            if (backwards)
            {
                neighbor.StartFrame = originalKeyFrameStart - Math.Sign(delta) + keyFrame.Duration - 1;
                finalDelta -= neighbor.Duration - 1;
            }
            else
            {
                neighbor.StartFrame = originalKeyFrameStart - Math.Sign(delta) - keyFrame.Duration + 1;
                finalDelta += neighbor.Duration - 1;
            }
        }

        return finalDelta;
    }
}
