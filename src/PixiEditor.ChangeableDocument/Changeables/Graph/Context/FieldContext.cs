using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public record struct FieldContext(VecD Position, VecI Size, bool HasContext)
{
    public FieldContext(VecD position, VecI size) : this(position, size, true) { }

    public static FieldContext NoContext => new(VecD.Zero, VecI.Zero, false);

    public void ThrowOnMissingContext()
    {
        if (!HasContext)
        {
            throw new NoNodeFieldContextException();
        }
    }
}
