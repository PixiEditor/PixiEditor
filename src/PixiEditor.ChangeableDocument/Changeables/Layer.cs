using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal abstract class Layer : StructureMember, IReadOnlyLayer
{
    public abstract ChunkyImage Rasterize(KeyFrameTime frameTime);
    public abstract void RemoveKeyFrame(Guid keyFrameGuid);
}
