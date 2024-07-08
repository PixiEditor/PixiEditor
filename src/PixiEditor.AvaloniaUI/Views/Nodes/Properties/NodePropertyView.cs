using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

namespace PixiEditor.AvaloniaUI.Views.Nodes.Properties;

public abstract class NodePropertyView : UserControl
{
    public NodeSocket InputSocket { get; private set; }
    public NodeSocket OutputSocket { get; private set; }
    protected override Type StyleKeyOverride => typeof(NodePropertyView);

    protected void SetValue(object value)
    {
        if (DataContext is NodePropertyViewModel viewModel)
        {
            viewModel.Value = value;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        InputSocket = e.NameScope.Find<NodeSocket>("PART_InputSocket");
        OutputSocket = e.NameScope.Find<NodeSocket>("PART_OutputSocket");
    }

    public Point GetSocketPoint(bool getInputSocket, Canvas canvas)
    {
        NodeSocket socket = getInputSocket ? InputSocket : OutputSocket;
        
        if (socket is null)
        {
            return default;
        }
        
        if(socket.ConnectPort is null)
        {
            return default;
        }
        
        Point? point = socket.ConnectPort.TranslatePoint(new Point(socket.ConnectPort.Bounds.Width / 2, socket.ConnectPort.Bounds.Height / 2), canvas);
        
        return point ?? default;
    }
}

public abstract class NodePropertyView<T> : NodePropertyView
{
    protected void SetValue(T value)
    {
        if (DataContext is NodePropertyViewModel<T> viewModel)
        {
            viewModel.Value = value;
        }
    }
}
