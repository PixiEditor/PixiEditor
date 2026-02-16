using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Converters;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Animations;

[TemplatePart("PART_ResizePanelRight", typeof(InputElement))]
[TemplatePart("PART_ResizePanelLeft", typeof(InputElement))]
[PseudoClasses(":selected")]
internal class KeyFrame : TemplatedControl
{
    public static readonly StyledProperty<CelViewModel> ItemProperty =
        AvaloniaProperty.Register<KeyFrame, CelViewModel>(
            nameof(Item));

    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<KeyFrame, double>(nameof(Scale), 100);

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<KeyFrame, bool>(
        nameof(IsSelected));

    public static readonly StyledProperty<double> MinProperty = AvaloniaProperty.Register<KeyFrame, double>(
        nameof(Min), 1);

    public static readonly StyledProperty<bool> IsCollapsedProperty = AvaloniaProperty.Register<KeyFrame, bool>(
        nameof(IsCollapsed));

    public static readonly StyledProperty<bool> IsDraggingProperty = AvaloniaProperty.Register<KeyFrame, bool>(
        nameof(IsDragging));

    public static readonly StyledProperty<double> PrecisePositionProperty = AvaloniaProperty.Register<KeyFrame, double>(
        nameof(PrecisePosition));

    public static readonly StyledProperty<ICommand> BeginDragCommandProperty =
        AvaloniaProperty.Register<KeyFrame, ICommand>(
            nameof(BeginDragCommand));

    public static readonly StyledProperty<ICommand> EndDragCommandProperty =
        AvaloniaProperty.Register<KeyFrame, ICommand>(
            nameof(EndDragCommand));

    public ICommand EndDragCommand
    {
        get => GetValue(EndDragCommandProperty);
        set => SetValue(EndDragCommandProperty, value);
    }

    public ICommand BeginDragCommand
    {
        get => GetValue(BeginDragCommandProperty);
        set => SetValue(BeginDragCommandProperty, value);
    }

    public double PrecisePosition
    {
        get => GetValue(PrecisePositionProperty);
        set => SetValue(PrecisePositionProperty, value);
    }

    public bool IsDragging
    {
        get => GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }

    public bool IsCollapsed
    {
        get => GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public double Min
    {
        get => GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public CelViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public double Scale
    {
        get { return (double)GetValue(ScaleProperty); }
        set { SetValue(ScaleProperty, value); }
    }

    public ICommand SelectLayerCommand
    {
        get { return (ICommand)GetValue(SelectLayerCommandProperty); }
        set { SetValue(SelectLayerCommandProperty, value); }
    }

    public ICommand PreciseDragCommand
    {
        get { return (ICommand)GetValue(PreciseDragCommandProperty); }
        set { SetValue(PreciseDragCommandProperty, value); }
    }

    private InputElement _resizePanelRight;
    private InputElement _resizePanelLeft;
    private Border previewBorder;

    private double initialDragOffset;

    public static readonly StyledProperty<ICommand> PreciseDragCommandProperty =
        AvaloniaProperty.Register<KeyFrame, ICommand>("PreciseDragCommand");

    public static readonly StyledProperty<ICommand> SelectLayerCommandProperty =
        AvaloniaProperty.Register<KeyFrame, ICommand>("SelectLayerCommand");

    static KeyFrame()
    {
        IsSelectedProperty.Changed.Subscribe(IsSelectedChanged);
        IsCollapsedProperty.Changed.Subscribe(IsCollapsedChanged);
        ItemProperty.Changed.Subscribe(ItemChanged);
        IsDraggingProperty.Changed.Subscribe(IsDraggingChanged);
    }

    private static void IsDraggingChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not KeyFrame keyFrame)
        {
            return;
        }

        keyFrame.PseudoClasses.Set(":dragging", keyFrame.IsDragging);
        (keyFrame.Parent as Visual).ZIndex = keyFrame.IsDragging ? 500 : 0;
    }

    private static void ItemChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not KeyFrame keyFrame)
        {
            return;
        }

        if (e.OldValue is CelViewModel oldCel)
        {
            oldCel.PropertyChanged -= keyFrame.NewCelOnPropertyChanged;
        }

        if (e.NewValue is CelViewModel newCel)
        {
            newCel.PropertyChanged += keyFrame.NewCelOnPropertyChanged;
        }
    }

    private void NewCelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CelViewModel celViewModel)
        {
            return;
        }

        if (e.PropertyName == nameof(CelViewModel.DurationBindable))
        {
            PseudoClasses.Set(":long", celViewModel.DurationBindable > 1);
        }

        if (e.PropertyName == nameof(CelViewModel.IsDragging))
        {
            celViewModel.PrecisePosition = FrameToMousePos(celViewModel.StartFrameBindable);
        }
    }

    public KeyFrame()
    {
        PointerPressed += (sender, args) =>
        {
            if (args.Source is Control { DataContext: CelViewModel celViewModel })
            {
                SelectLayerCommand?.Execute(celViewModel.LayerGuid);
            }
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _resizePanelRight = e.NameScope.Find<InputElement>("PART_ResizePanelRight");
        _resizePanelLeft = e.NameScope.Find<InputElement>("PART_ResizePanelLeft");

        previewBorder = e.NameScope.Find<Border>("PreviewBorder");

        _resizePanelRight.PointerPressed += CapturePointer;
        _resizePanelRight.PointerMoved += ResizePanelRightOnPointerMoved;

        _resizePanelLeft.PointerPressed += CapturePointer;
        _resizePanelLeft.PointerMoved += ResizePanelLeftOnPointerMoved;

        _resizePanelLeft.PointerCaptureLost += UpdateKeyFrame;
        _resizePanelRight.PointerCaptureLost += UpdateKeyFrame;

        if (Item is not CelGroupViewModel)
        {
            MultiBinding marginBinding = new MultiBinding
            {
                Converter = new DurationToMarginConverter(),
                Bindings =
                {
                    new Binding("StartFrameBindable") { Source = Item },
                    new Binding("Min") { Source = this },
                    new Binding("Scale") { Source = this },
                    new Binding("IsDragging") { Source = Item },
                    new Binding("PrecisePosition") { Source = this },
                }
            };

            ContentPresenter contentPresenter = this.FindAncestorOfType<ContentPresenter>();
            contentPresenter.Bind(MarginProperty, marginBinding);

            PseudoClasses.Set(":long", Item.DurationBindable > 1);
        }
    }

    private void CapturePointer(object? sender, PointerPressedEventArgs e)
    {
        if (Item is null || e.Handled)
        {
            return;
        }

        e.PreventGestureRecognition();
        e.Pointer.Capture(sender as IInputElement);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (Item is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (!IsDragging)
        {
            IsDragging = true;
            e.Pointer.Capture(this);
            initialDragOffset = e.GetPosition(previewBorder).X - 30;
            BeginDragCommand.Execute(Item);
        }

        double x = e.GetPosition(this.FindAncestorOfType<Border>()).X - 30;
        double delta = x - initialDragOffset - PrecisePosition;
        PrecisePosition = x - initialDragOffset;
        PreciseDragCommand.Execute((Item, delta));
    }

    override protected void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        IsDragging = false;
        EndDragCommand.Execute(Item);
    }

    private void ResizePanelRightOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Item is null)
        {
            return;
        }

        if (e.GetCurrentPoint(_resizePanelRight).Properties.IsLeftButtonPressed)
        {
            Item.ChangeFrameLength(Item.StartFrameBindable, MousePosToFrame(e) - Item.StartFrameBindable + 1);
        }

        e.Handled = true;
    }

    private void ResizePanelLeftOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Item is null)
        {
            return;
        }

        if (e.GetCurrentPoint(_resizePanelLeft).Properties.IsLeftButtonPressed)
        {
            int frame = MousePosToFrame(e);

            if (frame >= Item.StartFrameBindable + Item.DurationBindable)
            {
                frame = Item.StartFrameBindable + Item.DurationBindable - 1;
            }

            int oldStartFrame = Item.StartFrameBindable;
            Item.ChangeFrameLength(frame, Item.DurationBindable + oldStartFrame - frame);
        }

        e.Handled = true;
    }

    private int MousePosToFrame(PointerEventArgs e, bool round = true)
    {
        // 30 is a left visual padding on the timeline. TODO: Make it less...hardcoded
        double x = e.GetPosition(this.FindAncestorOfType<Border>()).X - 30;
        return MousePosToFrame(x, round);
    }

    private int MousePosToFrame(double x, bool round = true)
    {
        int frame;
        if (round)
        {
            frame = (int)Math.Round(x / Scale) + 1;
        }
        else
        {
            frame = (int)Math.Floor(x / Scale) + 1;
        }

        frame = Math.Max(1, frame);
        return frame;
    }

    private double FrameToMousePos(int frame)
    {
        return (frame - 1) * Scale;
    }

    private void UpdateKeyFrame(object? sender, PointerCaptureLostEventArgs e)
    {
        if (Item is null)
        {
            return;
        }

        Item.EndChangeFrameLength();
    }

    private static void IsSelectedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not KeyFrame keyFrame)
        {
            return;
        }

        keyFrame.PseudoClasses.Set(":selected", keyFrame.IsSelected);
    }

    private static void IsCollapsedChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not KeyFrame keyFrame)
        {
            return;
        }

        keyFrame.PseudoClasses.Set(":collapsed", keyFrame.IsCollapsed);
    }
}
