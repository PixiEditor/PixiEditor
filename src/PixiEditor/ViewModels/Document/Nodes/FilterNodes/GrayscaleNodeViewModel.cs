using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

[NodeViewModel("GRAYSCALE_FILTER_NODE", "FILTERS", PixiPerfectIcons.Ghost)]
internal class GrayscaleNodeViewModel : NodeViewModel<GrayscaleNode>
{
    private INodePropertyHandler customWeightsProp;
    public override void OnInitialized()
    {
        var modeProp = Inputs.FirstOrDefault(x => x.PropertyName == "Mode");
        customWeightsProp = Inputs.FirstOrDefault(x => x.PropertyName == "CustomWeight");
        modeProp.ValueChanged += ModePropOnValueChanged;

        customWeightsProp.IsVisible = modeProp.Value is GrayscaleNode.GrayscaleMode.Custom;
    }

    private void ModePropOnValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        customWeightsProp.IsVisible = args.NewValue is GrayscaleNode.GrayscaleMode.Custom;
    }
}
