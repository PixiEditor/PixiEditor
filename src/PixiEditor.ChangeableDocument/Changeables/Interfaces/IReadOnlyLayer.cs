using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyLayer : IReadOnlyStructureMember
{
    public ChunkyImage Rasterize(KeyFrameTime frameTime);
    public void RemoveKeyFrame(Guid keyFrameGuid);
    public void SetKeyFrameLength(Guid keyFrameGuid, int startFrame, int duration);
}
