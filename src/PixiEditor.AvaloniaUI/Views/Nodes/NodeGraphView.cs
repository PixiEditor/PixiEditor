using System.Collections.ObjectModel;
using Avalonia;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeGraphView : Zoombox.Zoombox
{
    public static readonly StyledProperty<ObservableCollection<NodeViewModel>> NodesProperty = AvaloniaProperty.Register<NodeGraphView, ObservableCollection<NodeViewModel>>(
        nameof(Nodes));

    public ObservableCollection<NodeViewModel> Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(NodeGraphView);

    public NodeGraphView()
    {
        NodeViewModel node = new NodeViewModel()
        {
            Name = "Node 1",
            X = 100,
            Y = 100,
            Inputs = new ObservableCollection<NodePropertyViewModel>()
        };
        
        node.Inputs.Add(new ImageNodePropertyViewModel(node) { Name = "Input 1" });

        Nodes =
        [
            node,
        ];
    }
}

