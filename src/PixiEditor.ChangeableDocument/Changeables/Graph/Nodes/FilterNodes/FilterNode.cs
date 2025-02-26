using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public abstract class FilterNode : Node
{
    public OutputProperty<Filter> Output { get; }

    public InputProperty<Filter?> Input { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    public FilterNode()
    {
        Output = CreateOutput<Filter>(nameof(Output), "FILTERS", null);
        Input = CreateInput<Filter>(nameof(Input), "PREVIOUS", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        var colorFilter = GetColorFilter();
        var imageFilter = GetImageFilter();

        if (colorFilter == null && imageFilter == null)
        {
            Output.Value = Input.Value;
            return;
        }

        var filter = Input.Value;

        Output.Value = filter == null ? new Filter(colorFilter, imageFilter) : filter.Add(colorFilter, imageFilter);
    }

    protected virtual ColorFilter? GetColorFilter() => null;

    protected virtual ImageFilter? GetImageFilter() => null;
}
