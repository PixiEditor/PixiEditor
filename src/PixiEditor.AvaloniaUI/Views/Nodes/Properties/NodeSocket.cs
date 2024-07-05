using Avalonia;
using Avalonia.Controls.Primitives;

namespace PixiEditor.AvaloniaUI.Views.Nodes.Properties;

public class NodeSocket : TemplatedControl
{
    public static readonly StyledProperty<bool> IsInputProperty = AvaloniaProperty.Register<NodeSocket, bool>("IsInput");
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<NodeSocket, string>("Label");

    public bool IsInput
    {
        get { return (bool)GetValue(IsInputProperty); }
        set { SetValue(IsInputProperty, value); }
    }

    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }
}

