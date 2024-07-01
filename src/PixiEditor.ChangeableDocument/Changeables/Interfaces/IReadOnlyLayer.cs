using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyLayer : IReadOnlyStructureMember
{
    public ChunkyImage Rasterize(KeyFrameTime frameTime);
}
