using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter", "APPLY_FILTER_NODE", Category = "FILTERS")]
public class ApplyFilterNode : Node
{
    private Paint _paint = new();
    
    
    public OutputProperty<Texture?> Output { get; }

    public InputProperty<Texture?> Input { get; }
    
    public InputProperty<Filter?> Filter { get; }
    
    private Texture _workingSurface;

    public ApplyFilterNode()
    {
        Output = CreateOutput<Texture>(nameof(Output), "IMAGE", null);
        Input = CreateInput<Texture>(nameof(Input), "IMAGE", null);
        Filter = CreateInput<Filter>(nameof(Filter), "FILTER", null);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        if (Input.Value is not { } input)
        {
            return null;
        }
        
        _paint.SetFilters(Filter.Value);
        
        _workingSurface = RequestTexture(0, input.Size, true);
        
        _workingSurface.DrawingSurface.Canvas.DrawSurface(input.DrawingSurface, 0, 0, _paint);

        Output.Value = _workingSurface;
        return _workingSurface;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
