using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("GRADIENT_NODE", "COLOR", null)]
internal class GradientNodeViewModel : NodeViewModel<GradientNode>
{
    public override void OnInitialized()
    {
        Inputs.CollectionChanged += InputsOnCollectionChanged;
        AdjustNames(Inputs);
    }

    private void InputsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AdjustNames(e.NewItems);
    }

    private static void AdjustNames(IList inputs)
    {
        if (inputs == null) return;
        foreach (var eNewItem in inputs)
        {
            if(eNewItem is NodePropertyViewModel input)
            {
                if (input.DisplayName == "COLOR_STOP_COLOR")
                {
                    int index = int.TryParse(input.PropertyName.Split('_').LastOrDefault(), out var parsed) ? parsed : -1;
                    if (index != -1)
                    {
                        input.DisplayName = new LocalizedString("COLOR_STOP_COLOR", index);
                    }
                }
                else if (input.DisplayName == "COLOR_STOP_POSITION")
                {
                    int index = int.TryParse(input.PropertyName.Split('_').LastOrDefault(), out var parsed) ? parsed : -1;
                    if (index != -1)
                    {
                        input.DisplayName = new LocalizedString("COLOR_STOP_POSITION", index);
                    }
                }
            }
        }
    }
}
