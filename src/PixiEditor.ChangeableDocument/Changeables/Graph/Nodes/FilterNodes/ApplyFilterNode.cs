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

        var workingSurface = new Texture(input.Size);
        
        workingSurface.DrawingSurface.Canvas.DrawSurface(input.DrawingSurface, 0, 0, _paint);

        Output.Value = workingSurface;
        return workingSurface;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
