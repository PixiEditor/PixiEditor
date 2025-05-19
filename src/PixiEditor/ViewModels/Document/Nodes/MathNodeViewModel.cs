using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.Helpers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("MATH_NODE", "NUMBERS", "\uE80E")]
internal class MathNodeViewModel : NodeViewModel<MathNode>
{
    private GenericEnumPropertyViewModel Mode { get; set; }
    
    private NodePropertyViewModel X { get; set; }
    
    private NodePropertyViewModel Y { get; set; }
    
    private NodePropertyViewModel Z { get; set; }
    
    public override void OnInitialized()
    {
        Mode = FindInputProperty("Mode") as GenericEnumPropertyViewModel;
        X = FindInputProperty("X");
        Y = FindInputProperty("Y");
        Z = FindInputProperty("Z");
        
        Mode.ValueChanged += (_, _) => ModeChanged();
        ModeChanged();
    }

    private void ModeChanged()
    {
        if (Mode.Value is not MathNodeMode mode)
            return;

        DisplayName = mode.GetDescription();
        Y.IsVisible = mode.UsesYValue();
        Z.IsVisible = mode.UsesZValue();

        var (x, y, z) = mode.GetNaming();

        x = new LocalizedString(x);
        y = new LocalizedString(y);
        z = new LocalizedString(z);

        X.DisplayName = x;
        Y.DisplayName = y;
        Z.DisplayName = z;
    }
}
