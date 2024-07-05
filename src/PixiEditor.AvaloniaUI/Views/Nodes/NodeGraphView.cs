using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.ViewModels.Nodes.Properties;

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

