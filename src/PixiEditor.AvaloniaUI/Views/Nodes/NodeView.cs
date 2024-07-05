using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.Views.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeView : TemplatedControl
{
    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<NodeView, string>(
        nameof(DisplayName), "Node");

    public static readonly StyledProperty<ObservableRangeCollection<INodePropertyHandler>> InputsProperty = AvaloniaProperty.Register<NodeView, ObservableRangeCollection<INodePropertyHandler>>(
        nameof(Inputs));
    
    public static readonly StyledProperty<ObservableRangeCollection<INodePropertyHandler>> OutputsProperty = AvaloniaProperty.Register<NodeView, ObservableRangeCollection<INodePropertyHandler>>(
        nameof(Outputs));

    public static readonly StyledProperty<Surface> ResultPreviewProperty = AvaloniaProperty.Register<NodeView, Surface>(
        nameof(ResultPreview));

    public Surface ResultPreview
    {
        get => GetValue( ResultPreviewProperty);
        set => SetValue( ResultPreviewProperty, value);
    }

    public ObservableRangeCollection<INodePropertyHandler> Outputs
    {
        get => GetValue(OutputsProperty);
        set => SetValue(OutputsProperty, value);
    }

    public ObservableRangeCollection<INodePropertyHandler> Inputs
    {
        get => GetValue(InputsProperty);
        set => SetValue(InputsProperty, value);
    }

    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public Point GetSocketPoint(INodePropertyHandler property, Canvas canvas)
    {
        NodePropertyView propertyView = this.GetVisualDescendants().OfType<NodePropertyView>().FirstOrDefault(x => x.DataContext == property);
        
        if (propertyView is null)
        {
            return default;
        }

        return propertyView.GetSocketPoint(property.IsInput, canvas);
    }
}
