using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("MATH_NODE", "NUMBERS", "\ue90e")]
internal class MathNodeViewModel : NodeViewModel<MathNode>
{
    private GenericEnumPropertyViewModel Mode { get; set; }
    
    private NodePropertyViewModel Y { get; set; }
    
    public override void OnInitialized()
    {
        Mode = FindInputProperty("Mode") as GenericEnumPropertyViewModel;
        Y = FindInputProperty("Y");
        
        Mode.ValueChanged += ModeChanged;
    }

    private void ModeChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        if (Mode.Value is not MathNodeMode mode)
            return;
        
        Y.IsVisible = mode.UsesYValue();
    }
}
