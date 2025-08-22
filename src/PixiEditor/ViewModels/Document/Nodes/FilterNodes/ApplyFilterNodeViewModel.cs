using System.Collections.Specialized;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

[NodeViewModel("APPLY_FILTER_NODE", "FILTERS", PixiPerfectIcons.Magic)]
internal class ApplyFilterNodeViewModel : NodeViewModel<ApplyFilterNode>
{
    private NodePropertyViewModel MaskInput { get; set; }
    
    private NodePropertyViewModel MaskInvertInput { get; set; }

    public override void OnInitialized()
    {
        MaskInput = FindInputProperty("Mask");
        MaskInvertInput = FindInputProperty("InvertMask");
        
        UpdateInvertVisible();
        MaskInput.ConnectedOutputChanged += (_, _) => UpdateInvertVisible();
    }

    private void UpdateInvertVisible()
    {
        MaskInvertInput.IsVisible = MaskInput.ConnectedOutput != null;
    }
}
