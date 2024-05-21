using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal abstract class Layer : StructureMember, IReadOnlyLayer
{
    public abstract ChunkyImage Rasterize();
}
