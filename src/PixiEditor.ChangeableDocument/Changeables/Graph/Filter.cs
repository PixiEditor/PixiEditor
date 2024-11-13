using System.Diagnostics.Contracts;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public abstract class Filter(Filter? previous)
{
    public Filter? Previous { get; } = previous;

    public void Apply(DrawingSurface surface)
    {
        Previous?.Apply(surface);
        
        DoApply(surface);
    }
    
    protected abstract void DoApply(DrawingSurface surface);
}
