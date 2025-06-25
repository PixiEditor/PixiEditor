using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ViewModels.Nodes;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Nodes;

internal class ConnectionView : TemplatedControl
{
    public static readonly StyledProperty<NodePropertyViewModel> InputPropertyProperty =
        AvaloniaProperty.Register<ConnectionView, NodePropertyViewModel>(
            nameof(InputProperty));

    public static readonly StyledProperty<NodePropertyViewModel> OutputPropertyProperty =
        AvaloniaProperty.Register<ConnectionView, NodePropertyViewModel>(
            nameof(OutputProperty));

    public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<ConnectionView, Point>(
        nameof(StartPoint));
    
    public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<ConnectionView, Point>(
        nameof(EndPoint));

    public static readonly StyledProperty<VecD> InputNodePositionProperty = AvaloniaProperty.Register<ConnectionView, VecD>("InputNodePosition");
    public static readonly StyledProperty<VecD> OutputNodePositionProperty = AvaloniaProperty.Register<ConnectionView, VecD>("OutputNodePosition");

    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }
    
    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public NodePropertyViewModel InputProperty
    {
        get => GetValue(InputPropertyProperty);
        set => SetValue(InputPropertyProperty, value);
    }

    public NodePropertyViewModel OutputProperty
    {
        get => GetValue(OutputPropertyProperty);
        set => SetValue(OutputPropertyProperty, value);
    }

    public VecD InputNodePosition
    {
        get { return (VecD)GetValue(InputNodePositionProperty); }
        set { SetValue(InputNodePositionProperty, value); }
    }

    public VecD OutputNodePosition
    {
        get { return (VecD)GetValue(OutputNodePositionProperty); }
        set { SetValue(OutputNodePositionProperty, value); }
    }

    private Canvas? mainCanvas;

    static ConnectionView()
    {
        AffectsRender<ConnectionView>(InputPropertyProperty, OutputPropertyProperty);
        InputPropertyProperty.Changed.Subscribe(OnInputPropertyChanged);
        OutputPropertyProperty.Changed.Subscribe(OnOutputPropertyChanged);
        InputNodePositionProperty.Changed.Subscribe(OnInputNodePositionChanged);
        OutputNodePositionProperty.Changed.Subscribe(OnOutputNodePositionChanged);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Dispatcher.UIThread.Post(
            () =>
        {
            StartPoint = CalculateSocketPoint(InputProperty);
            EndPoint = CalculateSocketPoint(OutputProperty);
        }, DispatcherPriority.Render);
    }
    
    public void UpdateSocketPoints()
    {
        StartPoint = CalculateSocketPoint(InputProperty);
        EndPoint = CalculateSocketPoint(OutputProperty);
    }

    private Point CalculateSocketPoint(BindingValue<NodePropertyViewModel> argsNewValue)
    {
        NodePropertyViewModel property = argsNewValue.Value;
        if (this.VisualRoot is null)
        {
            return default;
        }

        Canvas canvas = mainCanvas ??= this.FindAncestorOfType<NodeGraphView>().FindDescendantOfType<Canvas>();

        if (property.Node is null || canvas is null)
        {
            return default;
        }

        NodeView nodeView = canvas.GetVisualDescendants().OfType<NodeView>()
            .FirstOrDefault(x => x.DataContext == property.Node);

        if (nodeView is null)
        {
            return default;
        }

        return nodeView.GetSocketPoint(property, canvas);
    }

    private static void OnInputPropertyChanged(AvaloniaPropertyChangedEventArgs<NodePropertyViewModel> args)
    {
        ConnectionView connectionView = args.Sender as ConnectionView;
        connectionView.StartPoint = connectionView.CalculateSocketPoint(args.NewValue);
    }

    private static void OnOutputPropertyChanged(AvaloniaPropertyChangedEventArgs<NodePropertyViewModel> args)
    {
        ConnectionView connectionView = args.Sender as ConnectionView;
        connectionView.EndPoint = connectionView.CalculateSocketPoint(args.NewValue);
    }
    
    private static void OnInputNodePositionChanged(AvaloniaPropertyChangedEventArgs<VecD> args)
    {
        ConnectionView connectionView = args.Sender as ConnectionView;
        connectionView.StartPoint = connectionView.CalculateSocketPoint(connectionView.InputProperty);
    }
    
    private static void OnOutputNodePositionChanged(AvaloniaPropertyChangedEventArgs<VecD> args)
    {
        ConnectionView connectionView = args.Sender as ConnectionView;
        connectionView.EndPoint = connectionView.CalculateSocketPoint(connectionView.OutputProperty);
    }
}
