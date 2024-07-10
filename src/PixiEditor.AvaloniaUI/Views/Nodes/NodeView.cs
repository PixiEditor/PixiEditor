using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.Views.Nodes.Properties;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

[PseudoClasses(":selected")]
public class NodeView : TemplatedControl
{
    public static readonly StyledProperty<INodeHandler> NodeProperty =
        AvaloniaProperty.Register<NodeView, INodeHandler>(
            nameof(Node));

    public static readonly StyledProperty<string> DisplayNameProperty = AvaloniaProperty.Register<NodeView, string>(
        nameof(DisplayName), "Node");

    public static readonly StyledProperty<ObservableRangeCollection<INodePropertyHandler>> InputsProperty =
        AvaloniaProperty.Register<NodeView, ObservableRangeCollection<INodePropertyHandler>>(
            nameof(Inputs));

    public static readonly StyledProperty<ObservableRangeCollection<INodePropertyHandler>> OutputsProperty =
        AvaloniaProperty.Register<NodeView, ObservableRangeCollection<INodePropertyHandler>>(
            nameof(Outputs));

    public static readonly StyledProperty<Surface> ResultPreviewProperty = AvaloniaProperty.Register<NodeView, Surface>(
        nameof(ResultPreview));

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<NodeView, bool>(
        nameof(IsSelected));

    public static readonly StyledProperty<ICommand> SelectNodeCommandProperty =
        AvaloniaProperty.Register<NodeView, ICommand>("SelectNodeCommand");

    public static readonly StyledProperty<ICommand> StartDragCommandProperty =
        AvaloniaProperty.Register<NodeView, ICommand>("StartDragCommand");

    public static readonly StyledProperty<ICommand> DragCommandProperty =
        AvaloniaProperty.Register<NodeView, ICommand>("DragCommand");

    public static readonly StyledProperty<ICommand> EndDragCommandProperty =
        AvaloniaProperty.Register<NodeView, ICommand>("EndDragCommand");

    public INodeHandler Node
    {
        get => GetValue(NodeProperty);
        set => SetValue(NodeProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Surface ResultPreview
    {
        get => GetValue(ResultPreviewProperty);
        set => SetValue(ResultPreviewProperty, value);
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

    public ICommand SelectNodeCommand
    {
        get { return (ICommand)GetValue(SelectNodeCommandProperty); }
        set { SetValue(SelectNodeCommandProperty, value); }
    }

    public ICommand StartDragCommand
    {
        get { return (ICommand)GetValue(StartDragCommandProperty); }
        set { SetValue(StartDragCommandProperty, value); }
    }

    public ICommand DragCommand
    {
        get { return (ICommand)GetValue(DragCommandProperty); }
        set { SetValue(DragCommandProperty, value); }
    }

    public ICommand EndDragCommand
    {
        get { return (ICommand)GetValue(EndDragCommandProperty); }
        set { SetValue(EndDragCommandProperty, value); }
    }

    public static readonly StyledProperty<ICommand> SocketDropCommandProperty =
        AvaloniaProperty.Register<NodeView, ICommand>(
            nameof(SocketDropCommand));

    public ICommand SocketDropCommand
    {
        get => GetValue(SocketDropCommandProperty);
        set => SetValue(SocketDropCommandProperty, value);
    }

    private bool captured;

    static NodeView()
    {
        IsSelectedProperty.Changed.Subscribe(NodeSelectionChanged);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetMouseButton(this) != MouseButton.Left)
            return;

        var originalSource = e.Source;
        e.Source = e.Source is NodeSocket socket ? socket : Node;
        if (SelectNodeCommand != null && SelectNodeCommand.CanExecute(e))
        {
            SelectNodeCommand.Execute(e);
        }

        if (StartDragCommand != null && StartDragCommand.CanExecute(e))
        {
            if (e.Source is not NodeSocket)
            {
                e.Pointer.Capture(this);
                captured = true;
            }

            StartDragCommand.Execute(e);
        }

        e.Source = originalSource;
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!Equals(e.Pointer.Captured, this) && e.Source is not NodeSocket socket)
            return;

        if (DragCommand != null && DragCommand.CanExecute(e))
        {
            DragCommand.Execute(e);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        NodeSocket socket = null;
        if (e.Source is NodeSocket sourceSocket)
        {
            socket = sourceSocket;
        }

        if (SocketDropCommand != null && SocketDropCommand.CanExecute(socket))
        {
            SocketDropCommand?.Execute(socket);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        if (!captured) return;

        var originalSource = e.Source;
        e.Source = Node;
        if (EndDragCommand != null && EndDragCommand.CanExecute(e))
        {
            EndDragCommand.Execute(e);
        }

        e.Source = originalSource;
        captured = false;
        e.Handled = true;
    }

    public Point GetSocketPoint(INodePropertyHandler property, Canvas canvas)
    {
        NodePropertyView propertyView = this.GetVisualDescendants().OfType<NodePropertyView>()
            .FirstOrDefault(x => x.DataContext == property);

        if (propertyView is null)
        {
            return default;
        }

        return propertyView.GetSocketPoint(property.IsInput, canvas);
    }

    private static void NodeSelectionChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is NodeView nodeView)
        {
            nodeView.PseudoClasses.Set(":selected", e.NewValue.Value);
        }
    }
}
