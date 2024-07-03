using Avalonia;
using Avalonia.Controls.Primitives;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

public class NodeView : TemplatedControl
{
    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<NodeView, string>(
        nameof(DisplayName), "Node");

    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }
}
