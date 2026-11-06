using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

internal abstract class FilterNodeViewModel<T> : NodeViewModel<T>, IFilterNodeHandler where T : Node
{
    public bool IsFilterEnabled
    {
        get
        {
            if (InputPropertyMap.TryGetValue(FilterNode.IsFilterEnabledProperty, out var isEnabled))
            {
                return isEnabled.Value as bool? ?? true;
            }

            return true;
        }
        set
        {
            if (InputPropertyMap.TryGetValue(FilterNode.IsFilterEnabledProperty, out var isEnabled))
            {
                isEnabled.Value = value;
            }
        }
    }
}
