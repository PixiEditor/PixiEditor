using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
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
    
    protected override void OnExecute(RenderContext context)
    {
        if (Input.Value is not { } input)
        {
            return;
        }
        
        _paint.SetFilters(Filter.Value);
        
        _workingSurface = RequestTexture(0, input.Size, true);
        
        _workingSurface.DrawingSurface.Canvas.DrawSurface(input.DrawingSurface, 0, 0, _paint);

        Output.Value = _workingSurface;
    }

    public override Node CreateCopy() => new ApplyFilterNode();
}
