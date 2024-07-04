using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeGraphView : Zoombox.Zoombox
{
    public static readonly StyledProperty<ObservableCollection<NodeViewModel>> NodesProperty = AvaloniaProperty.Register<NodeGraphView, ObservableCollection<NodeViewModel>>(
        nameof(Nodes));

    public static readonly StyledProperty<ObservableCollection<NodeConnectionViewModel>> ConnectionsProperty = AvaloniaProperty.Register<NodeGraphView, ObservableCollection<NodeConnectionViewModel>>(
        nameof(Connections));

    public ObservableCollection<NodeConnectionViewModel> Connections
    {
        get => GetValue(ConnectionsProperty);
        set => SetValue(ConnectionsProperty, value);
    }
    
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
            Outputs = new ObservableCollection<NodePropertyViewModel>()
        };
        
        NodeViewModel node2 = new NodeViewModel()
        {
            Name = "Node 2",
            X = 500,
            Y = 100,
            Inputs = new ObservableCollection<NodePropertyViewModel>()
        };
        
        node.Outputs.Add(new ImageNodePropertyViewModel(node)
        {
            Name = "Output 1",
            Node = node,
            IsInput = false,
        });
        
        node2.Inputs.Add(new ImageNodePropertyViewModel(node2)
        {
            Name = "Input 1",
            Node = node2,
            IsInput = true,
        });

        Nodes =
        [
            node,
            node2,
        ];
        
        Connections =
        [
            new NodeConnectionViewModel()
            {
                InputNode = node2,
                InputProperty = node2.Inputs[0],
                OutputNode = node,
                OutputProperty = node.Outputs[0],
            },
        ];
    }
}

