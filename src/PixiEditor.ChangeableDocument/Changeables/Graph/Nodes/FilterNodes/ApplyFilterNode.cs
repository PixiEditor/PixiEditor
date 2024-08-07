using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
public class ApplyFilterNode : Node
{
    private Paint _paint = new();
    
    public override string DisplayName { get; set; } = "APPLY_FILTER_NODE";
    
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

        if (_workingSurface == null || _workingSurface.Size != input.Size)
        {
            _workingSurface?.Dispose();
            _workingSurface = new Texture(input.Size);
            _workingSurface.DrawingSurface.Canvas.Clear();
        }
        
        _workingSurface.DrawingSurface.Canvas.DrawSurface(input.DrawingSurface, 0, 0, _paint);

        Output.Value = _workingSurface;
        return _workingSurface;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
