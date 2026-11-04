using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public abstract class FilterNode : Node, IIterativeRenderSupport
{
    public const string OutputPropertyName = "Output";
    public const string InputPropertyName = "Input";

    public OutputProperty<Filter> Output { get; }

    public InputProperty<Filter?> Input { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;
    public bool SupportsIterativeRendering => true;

    private ImageFilter? lastImageFilter;
    private ColorFilter? lastColorFilter;
    public FilterNode()
    {
        Output = CreateOutput<Filter>(OutputPropertyName, "FILTERS", null);
        Input = CreateInput<Filter>(InputPropertyName, "PREVIOUS", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        lastColorFilter?.Dispose();
        lastImageFilter?.Dispose();

        var colorFilter = GetColorFilter(context);
        var imageFilter = GetImageFilter(context);

        lastColorFilter = colorFilter;
        lastImageFilter = imageFilter;

        if (colorFilter == null && imageFilter == null)
        {
            Output.Value = Input.Value;
            return;
        }

        var filter = Input.Value;

        Output.Value = filter == null ? new Filter(colorFilter, imageFilter) : filter.Add(colorFilter, imageFilter);
    }

    protected virtual ColorFilter? GetColorFilter(RenderContext context) => null;

    protected virtual ImageFilter? GetImageFilter(RenderContext context) => null;
}
