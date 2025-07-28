using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.ChangeableDocument.Changes.Animation;

internal class KeyFrameLength_UpdateableChange : UpdateableChange
{
    public Guid KeyFrameGuid { get;  }
    public int StartFrame { get; private set; }
    public int Duration { get; private set; }
    
    private int originalStartFrame;
    private int originalDuration;
    
    [GenerateUpdateableChangeActions]
    public KeyFrameLength_UpdateableChange(Guid keyFrameGuid, int startFrame, int duration)
    {
        StartFrame = startFrame;
        Duration = duration;
        KeyFrameGuid = keyFrameGuid;
    }
    
    [UpdateChangeMethod]
    public void Update(int startFrame, int duration)
    {
        StartFrame = startFrame;
        Duration = duration;
    }
    
    public override bool InitializeAndValidate(Document target)
    {
        if (target.AnimationData.TryFindKeyFrame<KeyFrame>(KeyFrameGuid, out KeyFrame frame))
        {
            var node = target.FindNode<Node>(frame.NodeId);
            if (node is null)
            {
                return false;
            }

            if (node.KeyFrames.FirstOrDefault()?.KeyFrameGuid == frame.Id)
            {
                return false;
            }

            originalStartFrame = frame.StartFrame;
            originalDuration = frame.Duration;
            return true;
        }

        return false;
    }
    
    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);
        keyFrame.StartFrame = StartFrame;
        keyFrame.Duration = Duration;
        return new KeyFrameLength_ChangeInfo(KeyFrameGuid, StartFrame, Duration);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        if (originalStartFrame == StartFrame && originalDuration == Duration)
        {
            ignoreInUndo = true;
            return new None();
        }
        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);
        keyFrame.StartFrame = StartFrame;
        keyFrame.Duration = Duration;
        ignoreInUndo = false;
        return new KeyFrameLength_ChangeInfo(KeyFrameGuid, StartFrame, Duration);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        if (originalStartFrame == StartFrame && originalDuration == Duration)
        {
            return new None();
        }
        target.AnimationData.TryFindKeyFrame(KeyFrameGuid, out KeyFrame keyFrame);
        keyFrame.StartFrame = originalStartFrame;
        keyFrame.Duration = originalDuration;
        return new KeyFrameLength_ChangeInfo(KeyFrameGuid, originalStartFrame, originalDuration);
    }
    
    public override bool IsMergeableWith(Change other)
    {
        return other is KeyFrameLength_UpdateableChange change && change.KeyFrameGuid == KeyFrameGuid;
    }
}
