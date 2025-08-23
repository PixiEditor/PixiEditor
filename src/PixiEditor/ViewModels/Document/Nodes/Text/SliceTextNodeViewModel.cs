using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Text;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Text;

[NodeViewModel("SLICE_TEXT_NODE", "SHAPE", null)]
internal class SliceTextNodeViewModel : NodeViewModel<SliceTextNode>
{
    private NodePropertyViewModel<bool> _useLengthProperty;
    private NodePropertyViewModel _lengthProperty;
    
    public override void OnInitialized()
    {
        _useLengthProperty = FindInputProperty<bool>("UseLength");
        _lengthProperty = FindInputProperty("Length");
        
        _useLengthProperty.ValueChanged += UseLengthValueChanged;
    }

    private void UseLengthValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args) =>
        _lengthProperty.IsVisible = _useLengthProperty.Value;
}
