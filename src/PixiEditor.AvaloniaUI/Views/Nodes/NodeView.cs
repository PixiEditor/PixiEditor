using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

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
}
