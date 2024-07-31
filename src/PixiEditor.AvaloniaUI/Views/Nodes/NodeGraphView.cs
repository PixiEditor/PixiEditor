using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.AvaloniaUI.Views.Nodes.Properties;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.AvaloniaUI.Views.Nodes;

internal class NodeGraphView : Zoombox.Zoombox
{
    public static readonly StyledProperty<INodeGraphHandler> NodeGraphProperty =
        AvaloniaProperty.Register<NodeGraphView, INodeGraphHandler>(
            nameof(NodeGraph));

    public static readonly StyledProperty<ICommand> SelectNodeCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(SelectNodeCommand));

    public static readonly StyledProperty<ICommand> StartDraggingCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(StartDraggingCommand));

    public static readonly StyledProperty<ICommand> DraggedCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(DraggedCommand));

    public static readonly StyledProperty<ICommand> EndDragCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(EndDragCommand));

    public static readonly StyledProperty<ICommand> ChangeNodePosCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(ChangeNodePosCommand));

    public static readonly StyledProperty<ICommand> EndChangeNodePosCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(EndChangeNodePosCommand));

    public static readonly StyledProperty<string> SearchQueryProperty =
        AvaloniaProperty.Register<NodeGraphView, string>(
            nameof(SearchQuery));

    public static readonly StyledProperty<ObservableCollection<Type>> AllNodeTypesProperty =
        AvaloniaProperty.Register<NodeGraphView, ObservableCollection<Type>>(
            nameof(AllNodeTypes));

    public static readonly StyledProperty<ICommand> SocketDropCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(SocketDropCommand));

    public static readonly StyledProperty<ICommand> CreateNodeCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>("CreateNodeCommand");

    public static readonly StyledProperty<ICommand> ConnectPropertiesCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            "ConnectPropertiesCommand");

    public static readonly StyledProperty<ICommand> CreateNodeFromContextCommandProperty = AvaloniaProperty.Register<NodeGraphView, ICommand>(
        "CreateNodeFromContextCommand");

    public ICommand CreateNodeFromContextCommand
    {
        get => GetValue(CreateNodeFromContextCommandProperty);
        set => SetValue(CreateNodeFromContextCommandProperty, value);
    }

    public ICommand ConnectPropertiesCommand
    {
        get => GetValue(ConnectPropertiesCommandProperty);
        set => SetValue(ConnectPropertiesCommandProperty, value);
    }

    public ObservableCollection<Type> AllNodeTypes
    {
        get => GetValue(AllNodeTypesProperty);
        set => SetValue(AllNodeTypesProperty, value);
    }

    public string SearchQuery
    {
        get => GetValue(SearchQueryProperty);
        set => SetValue(SearchQueryProperty, value);
    }

    public ICommand EndChangeNodePosCommand
    {
        get => GetValue(EndChangeNodePosCommandProperty);
        set => SetValue(EndChangeNodePosCommandProperty, value);
    }

    public ICommand ChangeNodePosCommand
    {
        get => GetValue(ChangeNodePosCommandProperty);
        set => SetValue(ChangeNodePosCommandProperty, value);
    }

    public ICommand EndDragCommand
    {
        get => GetValue(EndDragCommandProperty);
        set => SetValue(EndDragCommandProperty, value);
    }

    public ICommand DraggedCommand
    {
        get => GetValue(DraggedCommandProperty);
        set => SetValue(DraggedCommandProperty, value);
    }

    public ICommand StartDraggingCommand
    {
        get => GetValue(StartDraggingCommandProperty);
        set => SetValue(StartDraggingCommandProperty, value);
    }

    public ICommand SelectNodeCommand
    {
        get => GetValue(SelectNodeCommandProperty);
        set => SetValue(SelectNodeCommandProperty, value);
    }

    public INodeGraphHandler NodeGraph
    {
        get => GetValue(NodeGraphProperty);
        set => SetValue(NodeGraphProperty, value);
    }

    public List<INodeHandler> SelectedNodes => NodeGraph != null
        ? NodeGraph.AllNodes.Where(x => x.IsSelected).ToList()
        : new List<INodeHandler>();

    protected override Type StyleKeyOverride => typeof(NodeGraphView);

    public ICommand CreateNodeCommand
    {
        get { return (ICommand)GetValue(CreateNodeCommandProperty); }
        set { SetValue(CreateNodeCommandProperty, value); }
    }

    public ICommand SocketDropCommand
    {
        get => GetValue(SocketDropCommandProperty);
        set => SetValue(SocketDropCommandProperty, value);
    }

    private bool isDraggingNodes;
    private bool isDraggingConnection;
    private VecD clickPointOffset;

    private List<VecD> initialNodePositions;
    private INodePropertyHandler startConnectionProperty;
    private INodePropertyHandler endConnectionProperty;
    private INodeHandler startConnectionNode;
    private INodeHandler endConnectionNode;

    private Point startDragConnectionPoint;
    private ConnectionLine _previewConnectionLine;
    private NodeConnectionViewModel? _hiddenConnection;
    private Color _startingPropColor;
    private VecD _lastMouseClickPos;

    public NodeGraphView()
    {
        SelectNodeCommand = new RelayCommand<PointerPressedEventArgs>(SelectNode);
        StartDraggingCommand = new RelayCommand<PointerPressedEventArgs>(StartDragging);
        DraggedCommand = new RelayCommand<PointerEventArgs>(Dragged);
        EndDragCommand = new RelayCommand<PointerCaptureLostEventArgs>(EndDrag);
        SocketDropCommand = new RelayCommand<NodeSocket>(SocketDrop);
        CreateNodeFromContextCommand = new RelayCommand<Type>(CreateNodeType);

        AllNodeTypes = new ObservableCollection<Type>(GatherAssemblyTypes<Node>());
    }
    
    private void CreateNodeType(Type nodeType)
    {
        if (CreateNodeCommand != null && CreateNodeCommand.CanExecute(nodeType))
        {
            CreateNodeCommand.Execute((nodeType, _lastMouseClickPos));
            ((Control)this.GetVisualDescendants().FirstOrDefault()).ContextFlyout.Hide();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetMouseButton(this) == MouseButton.Left)
        {
            ClearSelection();
        }
        
        Point pos = e.GetPosition(this);
        _lastMouseClickPos = ToZoomboxSpace(new VecD(pos.X, pos.Y));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (isDraggingConnection)
        {
            UpdateConnectionEnd(e);
        }
    }

    private void UpdateConnectionEnd(PointerEventArgs e)
    {
        Point pos = e.GetPosition(this);
        VecD currentPoint = ToZoomboxSpace(new VecD(pos.X, pos.Y));

        NodeSocket? nodeSocket = e.Source as NodeSocket;
        
        if (nodeSocket != null)
        {
            Canvas canvas = nodeSocket.FindAncestorOfType<Canvas>();
            pos = nodeSocket.ConnectPort.TranslatePoint(
                new Point(nodeSocket.ConnectPort.Bounds.Width / 2, nodeSocket.ConnectPort.Bounds.Height / 2),
                canvas) ?? default;
            currentPoint = new VecD(pos.X, pos.Y);
        }


        if (_previewConnectionLine != null)
        {
            Point endPoint = new Point(currentPoint.X, currentPoint.Y);

            Color gradientStopFirstColor = _startingPropColor;
            Color gradientStopSecondColor =
                ((SolidColorBrush)nodeSocket?.SocketBrush)?.Color ?? gradientStopFirstColor;

            if (endPoint.X > startDragConnectionPoint.X)
            {
                _previewConnectionLine.StartPoint = endPoint;
                _previewConnectionLine.EndPoint = startDragConnectionPoint;
                (gradientStopFirstColor, gradientStopSecondColor) =
                    (gradientStopSecondColor, gradientStopFirstColor);
            }
            else
            {
                _previewConnectionLine.StartPoint = startDragConnectionPoint;
                _previewConnectionLine.EndPoint = endPoint;
            }

            _previewConnectionLine.LineBrush = new LinearGradientBrush()
            {
                GradientStops = new GradientStops()
                {
                    new GradientStop(gradientStopFirstColor, 0),
                    new GradientStop(gradientStopSecondColor, 1),
                }
            };
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (startConnectionProperty is { IsInput: true } && e.Source is not NodeSocket)
        {
            SocketDrop(null);
        }

        if (isDraggingConnection)
        {
            if (_previewConnectionLine != null)
            {
                _previewConnectionLine.IsVisible = false;
            }

            isDraggingConnection = false;
            _hiddenConnection = null;
        }
    }

    private IEnumerable<Type> GatherAssemblyTypes<T>()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
    }

    private void StartDragging(PointerPressedEventArgs e)
    {
        if (e.GetMouseButton(this) == MouseButton.Left)
        {
            if (e.Source is NodeSocket nodeSocket)
            {
                startConnectionProperty = nodeSocket.Property;
                startConnectionNode = nodeSocket.Node;
                isDraggingConnection = true;

                if (nodeSocket is { IsInput: true, Property.ConnectedOutput: not null })
                {
                    var conn = NodeGraph.Connections.FirstOrDefault(x => x.InputProperty == nodeSocket.Property);
                    if (conn != null)
                    {
                        _hiddenConnection = conn;
                        NodeGraph.Connections.Remove(conn);
                        NodeView view = FindNodeView(conn.OutputNode);
                        nodeSocket = view.GetSocket(conn.OutputProperty);
                    }
                }

                UpdatePreviewLine(nodeSocket);
            }
            else
            {
                isDraggingNodes = true;
                Point pt = e.GetPosition(this);
                clickPointOffset = ToZoomboxSpace(new VecD(pt.X, pt.Y));
                initialNodePositions = SelectedNodes.Select(x => x.PositionBindable).ToList();
            }
        }
    }

    private NodeView FindNodeView(INodeHandler node)
    {
        return this.GetVisualDescendants().OfType<NodeView>().FirstOrDefault(x => x.Node == node);
    }

    private void UpdatePreviewLine(NodeSocket nodeSocket)
    {
        Canvas canvas = nodeSocket.FindAncestorOfType<Canvas>();
        if (_previewConnectionLine == null)
        {
            _previewConnectionLine = new ConnectionLine();
            _previewConnectionLine.Thickness = 2;

            canvas.Children.Insert(0, _previewConnectionLine);
        }

        _previewConnectionLine.IsVisible = true;
        _startingPropColor = ((SolidColorBrush)nodeSocket.SocketBrush).Color;
        _previewConnectionLine.LineBrush = new LinearGradientBrush()
        {
            GradientStops = new GradientStops()
            {
                new GradientStop(_startingPropColor, 1),
            }
        };

        _previewConnectionLine.StartPoint = nodeSocket.ConnectPort.TranslatePoint(
            new Point(nodeSocket.ConnectPort.Bounds.Width / 2, nodeSocket.ConnectPort.Bounds.Height / 2),
            canvas) ?? default;
        _previewConnectionLine.EndPoint = _previewConnectionLine.StartPoint;
        startDragConnectionPoint = _previewConnectionLine.StartPoint;
    }

    private void Dragged(PointerEventArgs e)
    {
        if (isDraggingNodes)
        {
            Point pos = e.GetPosition(this);
            VecD currentPoint = ToZoomboxSpace(new VecD(pos.X, pos.Y));

            VecD delta = currentPoint - clickPointOffset;
            foreach (var node in SelectedNodes)
            {
                ChangeNodePosCommand?.Execute((node, initialNodePositions[SelectedNodes.IndexOf(node)] + delta));
            }
        }
    }

    private void EndDrag(PointerCaptureLostEventArgs e)
    {
        if (isDraggingNodes)
        {
            isDraggingNodes = false;
            EndChangeNodePosCommand?.Execute(null);
        }
    }

    private void SocketDrop(NodeSocket socket)
    {
        if (startConnectionProperty == null)
        {
            return;
        }

        (INodePropertyHandler, INodePropertyHandler) connection = (startConnectionProperty, null);
        if (socket != null)
        {
            endConnectionNode = socket.Node;
            endConnectionProperty = socket.Property;

            if (startConnectionNode == null || endConnectionNode == null || startConnectionProperty == null ||
                endConnectionProperty == null)
            {
                return;
            }

            connection = (startConnectionProperty, endConnectionProperty);

            if (startConnectionNode == endConnectionNode)
            {
                if (_hiddenConnection != null)
                {
                    NodeGraph.Connections.Add(_hiddenConnection);
                    _hiddenConnection = null;
                }

                return;
            }
        }

        if (ConnectPropertiesCommand != null && ConnectPropertiesCommand.CanExecute(connection))
        {
            ConnectPropertiesCommand.Execute(connection);
        }

        startConnectionProperty = null;
        endConnectionProperty = null;
        startConnectionNode = null;
        endConnectionNode = null;
    }

    private void SelectNode(PointerPressedEventArgs e)
    {
        if (e.Source is not NodeViewModel viewModel) return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (SelectedNodes.Contains(viewModel)) return;
        }
        // TODO: Add shift
        else if (!SelectedNodes.Contains(viewModel))
        {
            ClearSelection();
        }

        viewModel.IsSelected = true;
    }

    private void ClearSelection()
    {
        foreach (var node in SelectedNodes)
        {
            node.IsSelected = false;
        }
    }
}
