using System.Collections.ObjectModel;
using Avalonia;
using PixiEditor.AvaloniaUI.ViewModels.Document.Nodes;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeGraphView : Zoombox.Zoombox
{
    public static readonly StyledProperty<ObservableCollection<NodeViewModel>> NodesProperty = AvaloniaProperty.Register<NodeGraphView, ObservableCollection<NodeViewModel>>(
        nameof(Nodes), new ObservableCollection<NodeViewModel>()
        {
            new NodeViewModel() { Name = "Node 1", X = 100, Y = 100 },
            new NodeViewModel() { Name = "Node 2", X = 200, Y = 200 },
            new NodeViewModel() { Name = "Node 3", X = 300, Y = 300 }
        });

    public ObservableCollection<NodeViewModel> Nodes
    {
        get => GetValue(NodesProperty);
        set => SetValue(NodesProperty, value);
    }
    
    protected override Type StyleKeyOverride => typeof(NodeGraphView);
}

