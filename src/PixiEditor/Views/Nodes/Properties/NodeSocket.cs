using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Views.Nodes.Properties;

public class NodeSocket : TemplatedControl
{
    public static readonly StyledProperty<bool> IsInputProperty = AvaloniaProperty.Register<NodeSocket, bool>(nameof(IsInput));
    public static readonly StyledProperty<bool> IsFuncProperty = AvaloniaProperty.Register<NodeSocket, bool>(nameof(IsFunc));

    public static readonly StyledProperty<IBrush> SocketBrushProperty = AvaloniaProperty.Register<NodeSocket, IBrush>(nameof(SocketBrush));

    public IBrush SocketBrush
    {
        get => GetValue(SocketBrushProperty);
        set => SetValue(SocketBrushProperty, value);
    }

    public static readonly StyledProperty<INodeHandler> NodeProperty = AvaloniaProperty.Register<NodeSocket, INodeHandler>(nameof(Node));

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

    public bool IsFunc
    {
        get => GetValue(IsFuncProperty);
        set => SetValue(IsFuncProperty, value);
    }
    
    public Control ConnectPort { get; set; }

    public INodePropertyHandler Property { get; set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConnectPort = e.NameScope.Find<Control>("PART_ConnectPort");
        ConnectPort.PointerPressed += ConnectPortOnPointerPressed;
        ConnectPort.PointerReleased += ConnectPortOnPointerReleased;
        ConnectPort.PointerMoved += ConnectPortOnPointerMoved;
        ConnectPort.PointerEntered += ConnectPortOnPointerEntered;

    }

    private void ConnectPortOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Source = this;
        e.Pointer.Capture(null);
    }
    
    private void ConnectPortOnPointerMoved(object? sender, PointerEventArgs e)
    {
        e.Source = this;
    }

    private void ConnectPortOnPointerReleased(object? sender, PointerEventArgs e)
    {
        e.Source = this;
        e.Pointer.Capture(null);
    }

    private void ConnectPortOnPointerEntered(object? sender, PointerEventArgs e)
    {
        Property.UpdateComputedValue();
    }
}

