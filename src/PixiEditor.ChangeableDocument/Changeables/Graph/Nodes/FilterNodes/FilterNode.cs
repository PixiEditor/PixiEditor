using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.PaintImpl;

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
    
    protected override void OnExecute(RenderContext context)
    {
        Output.Value = GetFilter(Input.Value) ?? Input.Value;
    }

    protected abstract Filter? GetFilter(Filter? parent);
}
