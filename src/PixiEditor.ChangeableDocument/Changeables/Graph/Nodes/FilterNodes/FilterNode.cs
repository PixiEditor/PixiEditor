using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public abstract class FilterNode : Node
{
    public OutputProperty<Filter> Output { get; }
    
    public InputProperty<Filter?> Input { get; }
    
    public FilterNode()
    {
        Output = CreateOutput<Filter>(nameof(Output), "FILTERS", null);
        Input = CreateInput<Filter>(nameof(Input), "PREVIOUS", null);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        var colorFilter = GetColorFilter();
        var imageFilter = GetImageFilter();

        if (colorFilter == null && imageFilter == null)
        {
            Output.Value = Input.Value;
            return null;
        }

        var filter = Input.Value;

        Output.Value = filter == null ? new Filter(colorFilter, imageFilter) : filter.Add(colorFilter, imageFilter);
        
        return null;
    }

    protected virtual ColorFilter? GetColorFilter() => null;
    
    protected virtual ImageFilter? GetImageFilter() => null;
}
