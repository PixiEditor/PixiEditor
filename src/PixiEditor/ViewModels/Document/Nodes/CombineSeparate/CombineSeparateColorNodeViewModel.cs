using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal abstract class CombineSeparateColorNodeViewModel<T>(bool isInput) : NodeViewModel<T> where T : Node
{
    private GenericEnumPropertyViewModel Mode { get; set; }
    
    private NodePropertyViewModel<double> V1 { get; set; }

    private NodePropertyViewModel<double> V2 { get; set; }
    
    private NodePropertyViewModel<double> V3 { get; set; }

    public override void OnInitialized()
    {
        Mode = FindInputProperty("Mode") as GenericEnumPropertyViewModel;
        
        Mode.ValueChanged += OnModeChanged;

        if (isInput)
        {
            V1 = FindInputProperty<double>("R");
            V2 = FindInputProperty<double>("G");
            V3 = FindInputProperty<double>("B");
        }
        else
        {
            V1 = FindOutputProperty<double>("R");
            V2 = FindOutputProperty<double>("G");
            V3 = FindOutputProperty<double>("B");
        }
    }

    private void OnModeChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        if (args.NewValue == null) return;
        
        var mode = (CombineSeparateColorMode)args.NewValue;

        var (v1, v2, v3) = mode.GetLocalizedColorStringNames();

        V1.DisplayName = v1;
        V2.DisplayName = v2;
        V3.DisplayName = v3;
    }
}
