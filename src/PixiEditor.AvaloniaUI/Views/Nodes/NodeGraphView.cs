using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
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

    private bool isDraggingNodes;
    private VecD clickPointOffset;

    private List<VecD> initialNodePositions;

    public NodeGraphView()
    {
        SelectNodeCommand = new RelayCommand<PointerPressedEventArgs>(SelectNode);
        StartDraggingCommand = new RelayCommand<PointerPressedEventArgs>(StartDragging);
        DraggedCommand = new RelayCommand<PointerEventArgs>(Dragged);
        EndDragCommand = new RelayCommand<PointerCaptureLostEventArgs>(EndDrag);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetMouseButton(this) == MouseButton.Left)
            ClearSelection();
    }

    private void StartDragging(PointerPressedEventArgs e)
    {
        if (e.GetMouseButton(this) == MouseButton.Left)
        {
            isDraggingNodes = true;
            Point pt = e.GetPosition(this);
            clickPointOffset = ToZoomboxSpace(new VecD(pt.X, pt.Y));
            initialNodePositions = SelectedNodes.Select(x => x.PositionBindable).ToList();
        }
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
        isDraggingNodes = false;
        EndChangeNodePosCommand?.Execute(null);
    }

    private void SelectNode(PointerPressedEventArgs e)
    {
        NodeViewModel viewModel = (NodeViewModel)e.Source;

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
