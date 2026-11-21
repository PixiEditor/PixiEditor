using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

public abstract class FilterNode : Node, IIterativeRenderSupport
{
    public const string OutputPropertyName = "Output";
    public const string InputPropertyName = "Input";
    public const string IsFilterEnabledProperty = "IsFilterEnabled";

    public OutputProperty<Filter> Output { get; }
    public InputProperty<Filter?> Input { get; }
    public InputProperty<bool> IsFilterEnabled { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;
    public virtual bool SupportsIterativeRendering => true;

    public FilterNode()
    {
        Output = CreateOutput<Filter>(OutputPropertyName, "FILTERS", null);
        Input = CreateInput<Filter>(InputPropertyName, "PREVIOUS", null);
        IsFilterEnabled = CreateInput<bool>(IsFilterEnabledProperty, "IS_FILTER_ENABLED_PROPERTY", true);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (!IsFilterEnabled.Value)
        {
            if (Input.Value != null)
            {
                Output.Value = Input.Value;
            }
            else
            {
                Output.Value = null;
            }

            return;
        }

        var colorFilter = GetColorFilter(context);
        var imageFilter = GetImageFilter(context);

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
