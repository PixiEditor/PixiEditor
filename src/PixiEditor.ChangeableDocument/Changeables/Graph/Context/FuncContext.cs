using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class FuncContext
{
    public static FuncContext NoContext { get; } = new();
    
    public VecD Position { get; private set; }
    public VecI Size { get; private set; }
    public bool HasContext { get; private set; }

    public void ThrowOnMissingContext()
    {
        if (!HasContext)
        {
            throw new NoNodeFuncContextException();
        }
    }

    public void UpdateContext(VecD position, VecI size)
    {
        Position = position;
        Size = size;
        HasContext = true;
    }
}
