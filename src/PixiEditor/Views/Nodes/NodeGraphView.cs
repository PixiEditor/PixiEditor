using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.Document;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.Views.Nodes.Properties;
using PixiEditor.Zoombox;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Nodes;

[TemplatePart("PART_SelectionRectangle", typeof(Rectangle))]
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

    public static readonly StyledProperty<ObservableCollection<NodeTypeInfo>> AllNodeTypeInfosProperty =
        AvaloniaProperty.Register<NodeGraphView, ObservableCollection<NodeTypeInfo>>(
            nameof(AllNodeTypeInfos));

    public static readonly StyledProperty<ICommand> SocketDropCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            nameof(SocketDropCommand));

    public static readonly StyledProperty<ICommand> CreateNodeCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>("CreateNodeCommand");

    public static readonly StyledProperty<ICommand> ConnectPropertiesCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
            "ConnectPropertiesCommand");

    public static readonly StyledProperty<ICommand> CreateNodeFromContextCommandProperty =
        AvaloniaProperty.Register<NodeGraphView, ICommand>(
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

    public ObservableCollection<NodeTypeInfo> AllNodeTypeInfos
    {
        get => GetValue(AllNodeTypeInfosProperty);
        set => SetValue(AllNodeTypeInfosProperty, value);
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
        ? NodeGraph.AllNodes.Where(x => x.IsNodeSelected).ToList()
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

    public int ActiveFrame
    {
        get { return (int)GetValue(ActiveFrameProperty); }
        set { SetValue(ActiveFrameProperty, value); }
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
    private Point _lastMousePos;

    private ItemsControl nodeItemsControl;
    private ItemsControl connectionItemsControl;
    private Rectangle selectionRectangle;

    private List<INodeHandler> selectedNodesOnStartDrag = new();

    private List<Control> nodeViewsCache = new();

    private bool isSelecting;

    public static readonly StyledProperty<int> ActiveFrameProperty =
        AvaloniaProperty.Register<NodeGraphView, int>("ActiveFrame");

    private Panel rootPanel;

    public NodeGraphView()
    {
        SelectNodeCommand = new RelayCommand<PointerPressedEventArgs>(SelectNode);
        StartDraggingCommand = new RelayCommand<PointerPressedEventArgs>(StartDragging);
        DraggedCommand = new RelayCommand<PointerEventArgs>(Dragged);
        EndDragCommand = new RelayCommand<PointerCaptureLostEventArgs>(EndDrag);
        SocketDropCommand = new RelayCommand<NodeSocket>(SocketDrop);
        CreateNodeFromContextCommand = new RelayCommand<NodeTypeInfo>(CreateNodeType);

        AllNodeTypes = new ObservableCollection<Type>(GatherAssemblyTypes<NodeViewModel>());
        AllNodeTypeInfos = new ObservableCollection<NodeTypeInfo>(AllNodeTypes.Select(x => new NodeTypeInfo(x)));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        nodeItemsControl = e.NameScope.Find<ItemsControl>("PART_Nodes");
        connectionItemsControl = e.NameScope.Find<ItemsControl>("PART_Connections");
        selectionRectangle = e.NameScope.Find<Rectangle>("PART_SelectionRectangle");

        rootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

        Dispatcher.UIThread.Post(() =>
        {
            nodeViewsCache = nodeItemsControl.ItemsPanelRoot.Children.ToList();
            HandleNodesAdded(nodeViewsCache);
        });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Dispatcher.UIThread.Post(
            () =>
            {
                rootPanel.Focus(NavigationMethod.Pointer);
            }, DispatcherPriority.Input);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        nodeItemsControl.ItemsPanelRoot.Children.CollectionChanged -= NodeItems_CollectionChanged;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (nodeItemsControl is { ItemsPanelRoot: not null })
        {
            nodeItemsControl.ItemsPanelRoot.Children.CollectionChanged += NodeItems_CollectionChanged;
            nodeViewsCache = nodeItemsControl.ItemsPanelRoot.Children.ToList();
            HandleNodesAdded(nodeViewsCache);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space && e.Source.Equals(rootPanel))
        {
            rootPanel.ContextFlyout?.ShowAt(rootPanel);
            e.Handled = true;
        }
    }

    private void NodeItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            HandleNodesAdded(e.NewItems);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (Control control in e.OldItems)
            {
                if (control is not ContentPresenter presenter)
                {
                    continue;
                }

                nodeViewsCache.Remove(presenter);

                presenter.PropertyChanged -= OnPresenterPropertyChanged;
                if (presenter.Content is NodeViewModel nvm)
                {
                    nvm.PropertyChanged -= Node_PropertyChanged;
                }

                if (presenter.Child == null)
                {
                    continue;
                }

                NodeView nodeView = (NodeView)presenter.Child;
                nodeView.PropertyChanged -= NodeView_PropertyChanged;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            nodeViewsCache.Clear();
        }
    }

    private void HandleNodesAdded(IList? items)
    {
        foreach (Control control in items)
        {
            if (control is not ContentPresenter presenter)
            {
                continue;
            }

            if (!nodeViewsCache.Contains(presenter))
            {
                nodeViewsCache.Add(presenter);
            }

            presenter.PropertyChanged += OnPresenterPropertyChanged;

            if (presenter.Content is NodeViewModel nvm)
            {
                nvm.PropertyChanged += Node_PropertyChanged;
            }

            if (presenter.Child == null)
            {
                continue;
            }

            NodeView nodeView = (NodeView)presenter.Child;
            nodeView.PropertyChanged += NodeView_PropertyChanged;
        }
    }

    private void OnPresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ContentPresenter.ChildProperty)
        {
            if (e.NewValue is NodeView nodeView)
            {
                nodeView.PropertyChanged += NodeView_PropertyChanged;
                nodeView.Node.PropertyChanged += Node_PropertyChanged;
            }
        }
    }

    private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeViewModel.PositionBindable))
        {
            if (sender is NodeViewModel node)
            {
                Dispatcher.UIThread.Post(
                    () =>
                    {
                        UpdateConnections(FindNodeView(node));
                    }, DispatcherPriority.Render);
            }
        }
    }

    private void CreateNodeType(NodeTypeInfo nodeType)
    {
        var type = nodeType.NodeType;
        if (CreateNodeCommand != null && CreateNodeCommand.CanExecute(type))
        {
            CreateNodeCommand.Execute((type, _lastMouseClickPos));
            ((Control)this.GetVisualDescendants().FirstOrDefault()).ContextFlyout.Hide();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetMouseButton(this) == MouseButton.Left)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                ZoomMode = ZoomboxMode.Move;
            }
            else
            {
                ClearSelection();
                isSelecting = true;
                selectionRectangle.IsVisible = true;
                ZoomMode = ZoomboxMode.Normal;
                e.Handled = true;
            }
        }
        else
        {
            ZoomMode = e.GetMouseButton(this) == MouseButton.Middle ? ZoomboxMode.Move : ZoomboxMode.Normal;
            isSelecting = false;
            selectionRectangle.IsVisible = false;
        }

        Point pos = e.GetPosition(this);
        _lastMousePos = pos;
        _lastMouseClickPos = ToZoomboxSpace(new VecD(pos.X, pos.Y));
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (isDraggingConnection)
        {
            UpdateConnectionEnd(e);
        }
        else if (isSelecting)
        {
            var pos = e.GetPosition(this);
            Point currentPoint = new Point(pos.X, pos.Y);

            float x = (float)Math.Min(_lastMousePos.X, currentPoint.X);
            float y = (float)Math.Min(_lastMousePos.Y, currentPoint.Y);
            float width = (float)Math.Abs(_lastMousePos.X - currentPoint.X);
            float height = (float)Math.Abs(_lastMousePos.Y - currentPoint.Y);

            selectionRectangle.Width = width;
            selectionRectangle.Height = height;
            Thickness margin = new Thickness(x, y, 0, 0);

            selectionRectangle.Margin = margin;


            VecD zoomboxSpacePos = ToZoomboxSpace(new VecD(x, y));
            VecD zoomboxSpaceSize = ToZoomboxSpace(new VecD(x + width, y + height));

            x = (float)zoomboxSpacePos.X;
            y = (float)zoomboxSpacePos.Y;
            width = (float)(zoomboxSpaceSize.X - zoomboxSpacePos.X);
            height = (float)(zoomboxSpaceSize.Y - zoomboxSpacePos.Y);

            Rect zoomboxSpaceRect = new Rect(x, y, width, height);
            ClearSelection();
            SelectWithinBounds(zoomboxSpaceRect);
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
                GetSocketColor(nodeSocket) ?? gradientStopFirstColor;

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
                    new GradientStop(gradientStopFirstColor, 0), new GradientStop(gradientStopSecondColor, 1),
                }
            };
        }
    }

    private void SelectWithinBounds(Rect rect)
    {
        foreach (var control in nodeViewsCache)
        {
            if (control.Bounds.Intersects(rect))
            {
                if (control is ContentPresenter { Child: NodeView nodeView })
                {
                    nodeView.Node.IsNodeSelected = true;
                }
            }
        }
    }

    private static Color? GetSocketColor(NodeSocket? nodeSocket)
    {
        if (nodeSocket == null)
        {
            return null;
        }

        if (nodeSocket.SocketBrush is SolidColorBrush solidColor)
        {
            return solidColor.Color;
        }

        if (nodeSocket.SocketBrush is GradientBrush gradientBrush)
        {
            return gradientBrush.GradientStops.FirstOrDefault()?.Color;
        }

        return null;
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

        if (isSelecting)
        {
            isSelecting = false;
            selectionRectangle.IsVisible = false;
            selectionRectangle.Width = 0;
            selectionRectangle.Height = 0;
        }

        if (e.Source is NodeView nodeView)
        {
            UpdateConnections(nodeView);
        }
    }

    private IEnumerable<Type> GatherAssemblyTypes<T>()
    {
        return typeof(T).Assembly.GetTypes()
            .Where(x => typeof(T).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });
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
                selectedNodesOnStartDrag = SelectedNodes.ToList();
                initialNodePositions = selectedNodesOnStartDrag.Select(x => x.PositionBindable).ToList();
            }
        }
    }

    private void NodeView_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty)
        {
            NodeView nodeView = (NodeView)sender!;
            UpdateConnections(nodeView);
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

            canvas.Children.Add(_previewConnectionLine);
        }

        _previewConnectionLine.IsVisible = true;
        _startingPropColor = GetSocketColor(nodeSocket) ?? Colors.White;
        _previewConnectionLine.LineBrush = new LinearGradientBrush()
        {
            GradientStops = new GradientStops() { new GradientStop(_startingPropColor, 1), }
        };

        _previewConnectionLine.StartPoint = nodeSocket.ConnectPort.TranslatePoint(
            new Point(nodeSocket.ConnectPort.Bounds.Width / 2, nodeSocket.ConnectPort.Bounds.Height / 2),
            canvas) ?? default;
        _previewConnectionLine.EndPoint = _previewConnectionLine.StartPoint;
        startDragConnectionPoint = _previewConnectionLine.StartPoint;
    }

    private void UpdateConnections(NodeView nodeView)
    {
        if (nodeView == null)
        {
            return;
        }

        foreach (NodePropertyView propertyView in nodeView.GetVisualDescendants().OfType<NodePropertyView>())
        {
            NodePropertyViewModel property = (NodePropertyViewModel)propertyView.DataContext;
            UpdateConnectionView(property);
        }
    }

    private void UpdateConnectionView(NodePropertyViewModel? propertyView)
    {
        foreach (var connection in connectionItemsControl.ItemsPanelRoot.Children)
        {
            if (connection is ContentPresenter contentPresenter)
            {
                ConnectionView connectionView = (ConnectionView)contentPresenter.FindDescendantOfType<ConnectionView>();

                if (connectionView == null)
                {
                    continue;
                }

                if (connectionView.InputProperty == propertyView || connectionView.OutputProperty == propertyView)
                {
                    connectionView.UpdateSocketPoints();
                    connectionView.InvalidateVisual();
                }
            }
        }
    }

    private void Dragged(PointerEventArgs e)
    {
        if (isDraggingNodes)
        {
            Point pos = e.GetPosition(this);
            VecD currentPoint = ToZoomboxSpace(new VecD(pos.X, pos.Y));

            VecD delta = currentPoint - clickPointOffset;
            ChangeNodePosCommand?.Execute((selectedNodesOnStartDrag, initialNodePositions[0] + delta));
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

        (INodePropertyHandler, INodePropertyHandler, INodePropertyHandler?) connection = (startConnectionProperty, null,
            null);
        if (socket != null)
        {
            endConnectionNode = socket.Node;
            endConnectionProperty = socket.Property;

            if (startConnectionNode == null || endConnectionNode == null || startConnectionProperty == null ||
                endConnectionProperty == null)
            {
                return;
            }

            connection = (startConnectionProperty, endConnectionProperty, null);

            if (startConnectionProperty.IsInput && endConnectionProperty.IsInput &&
                startConnectionProperty.ConnectedOutput != null)
            {
                connection = (startConnectionProperty.ConnectedOutput, endConnectionProperty, startConnectionProperty);
            }

            if (startConnectionNode == endConnectionNode)
            {
                if (startConnectionProperty != endConnectionProperty)
                {
                    connection = (endConnectionProperty, startConnectionProperty, null);
                }
                else
                {
                    return;
                }
            }
        }

        if (_hiddenConnection != null)
        {
            NodeGraph.Connections.Add(_hiddenConnection);
            _hiddenConnection = null;
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

        viewModel.IsNodeSelected = true;
    }

    private void ClearSelection()
    {
        foreach (var node in SelectedNodes)
        {
            node.IsNodeSelected = false;
        }
    }
}
