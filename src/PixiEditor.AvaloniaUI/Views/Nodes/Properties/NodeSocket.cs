using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Views.Nodes.Properties;

public class NodeSocket : TemplatedControl
{
    public static readonly StyledProperty<bool> IsInputProperty = AvaloniaProperty.Register<NodeSocket, bool>("IsInput");
    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<NodeSocket, string>("Label");

    public static readonly StyledProperty<IBrush> SocketBrushProperty = AvaloniaProperty.Register<NodeSocket, IBrush>(
        "SocketBrush");

    public IBrush SocketBrush
    {
        get => GetValue(SocketBrushProperty);
        set => SetValue(SocketBrushProperty, value);
    }

    public static readonly StyledProperty<INodeHandler> NodeProperty = AvaloniaProperty.Register<NodeSocket, INodeHandler>(
        "Node");

    public INodeHandler Node
    {
        get => GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

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
    
    public Control ConnectPort { get; set; }

    public INodePropertyHandler Property { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConnectPort = e.NameScope.Find<Control>("PART_ConnectPort");
        ConnectPort.PointerPressed += ConnectPortOnPointerPressed;
        ConnectPort.PointerReleased += ConnectPortOnPointerReleased;
    }

    private void ConnectPortOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Source = this;
        e.Pointer.Capture(null);
    }

    private void ConnectPortOnPointerReleased(object? sender, PointerEventArgs e)
    {
        e.Source = this;
        e.Pointer.Capture(null);
    }
}

