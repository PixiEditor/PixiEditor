using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Utility;

[NodeViewModel("RANDOM_NODE", "UTILITY", PixiPerfectIcons.Noise)]
internal class RandomNodeViewModel : NodeViewModel<RandomNode>
{
    public override void OnInitialized()
    {
        InputPropertyMap[RandomNode.TriggerPropertyName].ValueChanged += (property, args) =>
        {
            InputPropertyMap[RandomNode.InputTriggerPropertyName].IsVisible = property.Value is RandomTrigger.OnInputChanged;
        };

        InputPropertyMap[RandomNode.InputTriggerPropertyName].IsVisible = InputPropertyMap[RandomNode.TriggerPropertyName].Value is RandomTrigger.OnInputChanged;
    }
}
