using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.Views.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeView : TemplatedControl
{
    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<NodeView, string>(
        nameof(DisplayName), "Node");

    public static readonly StyledProperty<ObservableCollection<NodePropertyViewModel>> InputsProperty = AvaloniaProperty.Register<NodeView, ObservableCollection<NodePropertyViewModel>>(
        nameof(Inputs));
    
    public static readonly StyledProperty<ObservableCollection<NodePropertyViewModel>> OutputsProperty = AvaloniaProperty.Register<NodeView, ObservableCollection<NodePropertyViewModel>>(
        nameof(Outputs));

    public static readonly StyledProperty<Surface> ResultPreviewProperty = AvaloniaProperty.Register<NodeView, Surface>(
        nameof(ResultPreview));

    public Surface ResultPreview
    {
        get => GetValue( ResultPreviewProperty);
        set => SetValue( ResultPreviewProperty, value);
    }

    public ObservableCollection<NodePropertyViewModel> Outputs
    {
        get => GetValue(OutputsProperty);
        set => SetValue(OutputsProperty, value);
    }

    public ObservableCollection<NodePropertyViewModel> Inputs
    {
        get => GetValue(InputsProperty);
        set => SetValue(InputsProperty, value);
    }

    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public Point GetSocketPoint(NodePropertyViewModel property, Canvas canvas)
    {
        NodePropertyView propertyView = this.GetVisualDescendants().OfType<NodePropertyView>().FirstOrDefault(x => x.DataContext == property);
        
        if (propertyView is null)
        {
            return default;
        }

        return propertyView.GetSocketPoint(property.IsInput, canvas);
    }
}
