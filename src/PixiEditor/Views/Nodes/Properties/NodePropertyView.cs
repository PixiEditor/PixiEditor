using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Views.Nodes.Properties;

[PseudoClasses(":has-errors")]
public abstract class NodePropertyView : UserControl
{
    public static readonly StyledProperty<string> ErrorsProperty = AvaloniaProperty.Register<NodePropertyView, string>(
        nameof(Errors));

    public string Errors
    {
        get => GetValue(ErrorsProperty);
        set => SetValue(ErrorsProperty, value);
    }

    public NodeSocket InputSocket { get; private set; }
    public NodeSocket OutputSocket { get; private set; }
    protected override Type StyleKeyOverride => typeof(NodePropertyView);

    static NodePropertyView()
    {
        ErrorsProperty.Changed.Subscribe(OnErrorsChanged);
    }

    protected void SetValue(object value)
    {
        if (DataContext is NodePropertyViewModel viewModel)
        {
            viewModel.Value = value;
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is NodePropertyViewModel propertyHandler)
        {
            propertyHandler.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(NodePropertyViewModel.Errors))
                {
                    Errors = propertyHandler.Errors;
                }
            };
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        InputSocket = e.NameScope.Find<NodeSocket>("PART_InputSocket");
        OutputSocket = e.NameScope.Find<NodeSocket>("PART_OutputSocket");
        
        if(InputSocket is null || OutputSocket is null)
        {
            return;
        }

        INodePropertyHandler propertyHandler = DataContext as INodePropertyHandler;

        InputSocket.Property = propertyHandler;
        InputSocket.Node = propertyHandler.Node;
        OutputSocket.Node = propertyHandler.Node;
        OutputSocket.Property = propertyHandler;
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

    protected void HideSocket(bool hideInputSocket, bool hideOutputSocket)
    {
        if (hideInputSocket)
        {
            InputSocket.IsVisible = false;
        }

        if (hideOutputSocket)
        {
            OutputSocket.IsVisible = false;
        }
    }

    private static void OnErrorsChanged(AvaloniaPropertyChangedEventArgs<string> args)
    {
        if (args.Sender is NodePropertyView view)
        {
            view.PseudoClasses.Set(":has-errors", args.NewValue.HasValue && !string.IsNullOrEmpty(args.NewValue.Value));
        }
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
