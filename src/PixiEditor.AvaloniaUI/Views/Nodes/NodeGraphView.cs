using Avalonia;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeGraphView : Zoombox.Zoombox
{
    public static readonly StyledProperty<INodeGraphHandler> NodeGraphProperty = AvaloniaProperty.Register<NodeGraphView, INodeGraphHandler>(
        nameof(NodeGraph));

    public INodeGraphHandler NodeGraph
    {
        get => GetValue(NodeGraphProperty);
        set => SetValue(NodeGraphProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(NodeGraphView);

    public NodeGraphView()
    {
        
    }
}

