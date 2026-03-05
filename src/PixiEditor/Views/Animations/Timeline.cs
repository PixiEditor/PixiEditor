using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Animations;

[TemplatePart("PART_PlayToggle", typeof(ToggleButton))]
[TemplatePart("PART_TimelineSlider", typeof(TimelineSlider))]
[TemplatePart("PART_ContentGrid", typeof(Grid))]
[TemplatePart("PART_TimelineKeyFramesScroll", typeof(ScrollViewer))]
[TemplatePart("PART_TimelineHeaderScroll", typeof(ScrollViewer))]
[TemplatePart("PART_SelectionRectangle", typeof(Rectangle))]
[TemplatePart("PART_KeyFramesHost", typeof(ItemsControl))]
internal class Timeline : TemplatedControl, INotifyPropertyChanged
{
    private const float MarginMultiplier = 1.5f;

    public static readonly StyledProperty<KeyFrameCollection> KeyFramesProperty =
        AvaloniaProperty.Register<Timeline, KeyFrameCollection>(
            nameof(KeyFrames));

    public static readonly StyledProperty<int> ActiveFrameProperty =
        AvaloniaProperty.Register<Timeline, int>(nameof(ActiveFrame), 1);

    public static readonly StyledProperty<bool> IsPlayingProperty = AvaloniaProperty.Register<Timeline, bool>(
        nameof(IsPlaying));

    public static readonly StyledProperty<ICommand> NewKeyFrameCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(NewKeyFrameCommand));

    public static readonly StyledProperty<double> ScaleProperty = AvaloniaProperty.Register<Timeline, double>(
        nameof(Scale), 100);

    public static readonly StyledProperty<int> FpsProperty = AvaloniaProperty.Register<Timeline, int>(nameof(Fps), 60);

    public static readonly StyledProperty<Vector> ScrollOffsetProperty = AvaloniaProperty.Register<Timeline, Vector>(
        nameof(ScrollOffset));

    public static readonly StyledProperty<int> OnionFramesProperty = AvaloniaProperty.Register<Timeline, int>(
        nameof(OnionFrames), 1);

    public int OnionFrames
    {
        get => GetValue(OnionFramesProperty);
        set => SetValue(OnionFramesProperty, value);
    }

    public Vector ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    public double Scale
    {
        get => GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public ICommand NewKeyFrameCommand
    {
        get => GetValue(NewKeyFrameCommandProperty);
        set => SetValue(NewKeyFrameCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> DuplicateKeyFrameCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(DuplicateKeyFrameCommand));

    public static readonly StyledProperty<ICommand> DeleteKeyFrameCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(DeleteKeyFrameCommand));

    public static readonly StyledProperty<double> MinLeftOffsetProperty = AvaloniaProperty.Register<Timeline, double>(
        nameof(MinLeftOffset), 30);

    public static readonly StyledProperty<ICommand> ChangeKeyFramesLengthCommandProperty =
        AvaloniaProperty.Register<Timeline, ICommand>(
            nameof(ChangeKeyFramesLengthCommand));

    public static readonly StyledProperty<int> DefaultEndFrameProperty = AvaloniaProperty.Register<Timeline, int>(
        nameof(DefaultEndFrame));

    public static readonly StyledProperty<bool> OnionSkinningEnabledProperty =
        AvaloniaProperty.Register<Timeline, bool>(
            nameof(OnionSkinningEnabled));

    public static readonly StyledProperty<double> OnionOpacityProperty = AvaloniaProperty.Register<Timeline, double>(
        nameof(OnionOpacity), 50);

    public static readonly StyledProperty<bool> FallbackFramesToLayerImageProperty = AvaloniaProperty.Register<Timeline, bool>(
        nameof(FallbackFramesToLayerImage));

    public bool FallbackFramesToLayerImage
    {
        get => GetValue(FallbackFramesToLayerImageProperty);
        set => SetValue(FallbackFramesToLayerImageProperty, value);
    }

    public double OnionOpacity
    {
        get => GetValue(OnionOpacityProperty);
        set => SetValue(OnionOpacityProperty, value);
    }

    public bool OnionSkinningEnabled
    {
        get => GetValue(OnionSkinningEnabledProperty);
        set => SetValue(OnionSkinningEnabledProperty, value);
    }

    public int DefaultEndFrame
    {
        get => GetValue(DefaultEndFrameProperty);
        set => SetValue(DefaultEndFrameProperty, value);
    }

    public ICommand ChangeKeyFramesLengthCommand
    {
        get => GetValue(ChangeKeyFramesLengthCommandProperty);
        set => SetValue(ChangeKeyFramesLengthCommandProperty, value);
    }

    public double MinLeftOffset
    {
        get => GetValue(MinLeftOffsetProperty);
        set => SetValue(MinLeftOffsetProperty, value);
    }

    public ICommand DeleteKeyFrameCommand
    {
        get => GetValue(DeleteKeyFrameCommandProperty);
        set => SetValue(DeleteKeyFrameCommandProperty, value);
    }

    public ICommand DuplicateKeyFrameCommand
    {
        get => GetValue(DuplicateKeyFrameCommandProperty);
        set => SetValue(DuplicateKeyFrameCommandProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public KeyFrameCollection KeyFrames
    {
        get => GetValue(KeyFramesProperty);
        set => SetValue(KeyFramesProperty, value);
    }

    public int ActiveFrame
    {
        get { return (int)GetValue(ActiveFrameProperty); }
        set { SetValue(ActiveFrameProperty, value); }
    }

    public int Fps
    {
        get { return (int)GetValue(FpsProperty); }
        set { SetValue(FpsProperty, value); }
    }

    public IReadOnlyCollection<CelViewModel> SelectedKeyFrames => KeyFrames != null
        ? KeyFrames.SelectChildrenBy<CelViewModel>(x => x.IsSelected).ToList()
        : [];

    public int EndFrame => KeyFrames?.FrameCount > 0 ? KeyFrames.FrameCount - 1 : DefaultEndFrame;

    public ICommand DraggedKeyFrameCommand { get; }
    public ICommand ReleasedKeyFrameCommand { get; }
    public ICommand ClearSelectedKeyFramesCommand { get; }
    public ICommand PressedKeyFrameCommand { get; }

    public ICommand StepStartCommand { get; }
    public ICommand StepEndCommand { get; }
    public ICommand StepForwardCommand { get; }
    public ICommand StepBackCommand { get; }
    public ICommand PreciseDragKeyFrameCommand { get; }
    public ICommand BeginDragKeyFrameCommand { get; }
    public ICommand EndDragKeyFrameCommand { get; }

    private ToggleButton? _playToggle;
    private Grid? _contentGrid;
    private TimelineSlider? _timelineSlider;
    private ScrollViewer? _timelineKeyFramesScroll;
    private ScrollViewer? _timelineHeaderScroll;
    private Control? extendingElement;
    private Rectangle _selectionRectangle;
    private ItemsControl? _keyFramesHost;

    private Vector clickPos;

    private bool shouldClearNextSelection = true;
    private bool shouldShiftSelect = false;
    private CelViewModel clickedCel;
    private bool dragged;
    private bool draggedKeyFrameWasSelected;
    private Guid[] draggedKeyFrames;
    private int dragStartFrame;

    private double lastMovementPrecisePosition;


    public event PropertyChangedEventHandler? PropertyChanged;

    static Timeline()
    {
        KeyFramesProperty.Changed.Subscribe(OnKeyFramesChanged);
        DefaultEndFrameProperty.Changed.Subscribe(OnDefaultEndFrameChanged);
    }

    public Timeline()
    {
        PressedKeyFrameCommand = new RelayCommand<PointerPressedEventArgs>(KeyFramePressed);
        ClearSelectedKeyFramesCommand = new RelayCommand<CelViewModel>(ClearSelectedKeyFrames);
        DraggedKeyFrameCommand = new RelayCommand<PointerEventArgs>(KeyFramesDragged);
        ReleasedKeyFrameCommand = new RelayCommand<CelViewModel>(KeyFramesReleased);
        BeginDragKeyFrameCommand = new RelayCommand<CelViewModel>(BeginDragKeyFrames);
        PreciseDragKeyFrameCommand = new RelayCommand<(CelViewModel source, double delta)>(KeyFramesPreciseDragged);
        EndDragKeyFrameCommand = new RelayCommand<CelViewModel>(OnEndedPreciseDrag);

        StepStartCommand = new RelayCommand(() =>
        {
            var keyFramesWithinActiveFrame = KeyFrames.Where(x => x.IsVisible
                                                                  && x.StartFrameBindable < ActiveFrame)
                .SelectMany(x => x.Children).ToList();
            if (keyFramesWithinActiveFrame.Count > 0)
            {
                List<int> snapPoints = keyFramesWithinActiveFrame
                    .Select(x => x.StartFrameBindable + x.DurationBindable - 1).ToList();
                snapPoints.AddRange(KeyFrames.Select(x => x.StartFrameBindable));
                snapPoints.RemoveAll(x => x >= ActiveFrame);

                ActiveFrame = snapPoints.Max();
            }
            else
            {
                ActiveFrame = 1;
            }
        });

        StepEndCommand = new RelayCommand(() =>
        {
            var keyFramesWithinActiveFrame = KeyFrames.Where(x => x.IsVisible
                                                                  && x.StartFrameBindable + x.DurationBindable - 1 >
                                                                  ActiveFrame).SelectMany(x => x.Children).ToList();
            if (keyFramesWithinActiveFrame.Count > 0)
            {
                List<int> snapPoints = keyFramesWithinActiveFrame
                    .Select(x => x.StartFrameBindable + x.DurationBindable - 1).ToList();
                snapPoints.AddRange(KeyFrames.Select(x => x.StartFrameBindable));
                snapPoints.RemoveAll(x => x <= ActiveFrame);

                ActiveFrame = snapPoints.Min();
            }
            else
            {
                ActiveFrame = EndFrame;
            }
        });

        StepForwardCommand = new RelayCommand(() =>
        {
            ActiveFrame++;
        });

        StepBackCommand = new RelayCommand(() =>
        {
            if (ActiveFrame > 1)
            {
                ActiveFrame--;
            }
        });
    }

    private void BeginDragKeyFrames(CelViewModel celViewModel)
    {
        SelectKeyFrame(celViewModel, shouldClearNextSelection && !draggedKeyFrameWasSelected);

        foreach (var keyFrame in SelectedKeyFrames)
        {
            if (keyFrame != celViewModel)
            {
                keyFrame.IsDragging = true;
            }
        }
    }

    public void SelectKeyFrame(ICelHandler? keyFrame, bool clearSelection = true)
    {
        if (clearSelection)
        {
            ClearSelectedKeyFrames();
        }


        keyFrame?.Document.AnimationHandler.AddSelectedKeyFrame(keyFrame.Id);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
    }

    public bool DragAllSelectedKeyFrames(int delta)
    {
        bool canDrag = SelectedKeyFrames.All(x => x.StartFrameBindable + delta > 0);
        if (!canDrag)
        {
            return false;
        }

        var selected = SelectedKeyFrames;
        var ids = selected.Select(x => x.Id).ToArray();

        draggedKeyFrames = ids;

        ChangeKeyFramesLengthCommand.Execute((ids, delta, false));
        return true;
    }

    public void EndDragging()
    {
        if (dragged)
        {
            if (draggedKeyFrames is { Length: > 0 })
            {
                ChangeKeyFramesLengthCommand?.Execute((draggedKeyFrames.ToArray(), 0, true));
                foreach (var keyFrame in SelectedKeyFrames)
                {
                    keyFrame.IsDragging = false;
                }
            }
        }

        clickedCel = null;
    }

    private void OnEndedPreciseDrag(CelViewModel source)
    {
        foreach (var keyFrame in SelectedKeyFrames)
        {
            if (keyFrame != source)
            {
                keyFrame.IsDragging = false;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _playToggle = e.NameScope.Find<ToggleButton>("PART_PlayToggle");

        if (_playToggle != null)
        {
            _playToggle.Click += PlayToggleOnClick;
        }

        _contentGrid = e.NameScope.Find<Grid>("PART_ContentGrid");

        _timelineSlider = e.NameScope.Find<TimelineSlider>("PART_TimelineSlider");

        _timelineKeyFramesScroll = e.NameScope.Find<ScrollViewer>("PART_TimelineKeyFramesScroll");
        _timelineHeaderScroll = e.NameScope.Find<ScrollViewer>("PART_TimelineHeaderScroll");

        _selectionRectangle = e.NameScope.Find<Rectangle>("PART_SelectionRectangle");

        _timelineKeyFramesScroll.AddHandler(PointerWheelChangedEvent, (s, e) =>
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                return;
            }

            TimelineSliderOnPointerWheelChanged(s, e);
        }, RoutingStrategies.Tunnel);
        _timelineSlider.PointerWheelChanged += TimelineSliderOnPointerWheelChanged;
        _timelineKeyFramesScroll.ScrollChanged += TimelineKeyFramesScrollOnScrollChanged;
        _contentGrid.PointerPressed += ContentOnPointerPressed;
        _contentGrid.PointerMoved += ContentOnPointerMoved;
        _contentGrid.PointerCaptureLost += ContentOnPointerLost;

        extendingElement = new Control();
        extendingElement.SetValue(MarginProperty, new Thickness(0, 0, 0, 0));
        _contentGrid.Children.Add(extendingElement);

        _keyFramesHost = e.NameScope.Find<ItemsControl>("PART_KeyFramesHost");
    }

    private void KeyFramesReleased(CelViewModel? e)
    {
        if (!dragged)
        {
            if (shouldShiftSelect)
            {
                var lastSelected = SelectedKeyFrames.LastOrDefault();
                if (lastSelected != null)
                {
                    int startFrame = lastSelected.StartFrameBindable;
                    int endFrame = e.StartFrameBindable;
                    if (startFrame > endFrame)
                    {
                        (startFrame, endFrame) = (endFrame, startFrame);
                    }

                    int groupStartIndex = -1;
                    int groupEndIndex = -1;

                    for (int i = 0; i < KeyFrames.Count; i++)
                    {
                        if (KeyFrames[i].LayerGuid == lastSelected.LayerGuid)
                        {
                            groupStartIndex = i;
                        }

                        if (KeyFrames[i].LayerGuid == e.LayerGuid)
                        {
                            groupEndIndex = i;
                        }
                    }

                    if (groupStartIndex != -1 && groupEndIndex != -1 && groupStartIndex > groupEndIndex)
                    {
                        (groupStartIndex, groupEndIndex) = (groupEndIndex, groupStartIndex);
                    }

                    for (int i = groupStartIndex; i <= groupEndIndex; i++)
                    {
                        foreach (var keyFrame in KeyFrames[i].Children)
                        {
                            if (keyFrame.StartFrameBindable >= startFrame && keyFrame.StartFrameBindable <= endFrame)
                            {
                                SelectKeyFrame(keyFrame, false);
                            }
                        }
                    }
                }
            }

            SelectKeyFrame(e, shouldClearNextSelection);
            shouldClearNextSelection = true;
        }
        else
        {
            EndDragging();
        }

        dragged = false;
        clickedCel = null;
    }

    private void KeyFramesDragged(PointerEventArgs? e)
    {
        if (clickedCel == null) return;

        int frameUnderMouse = MousePosToFrame(e);

        bool movingBackwards = frameUnderMouse < dragStartFrame;

        int precisePositionFrame = movingBackwards ? MousePosToFrameCeil(clickedCel.PrecisePosition) : MousePosToFrame(clickedCel.PrecisePosition, true);
        int shiftedFrames = precisePositionFrame - clickedCel.StartFrameBindable;
        if ((shiftedFrames > 0 && movingBackwards) || (shiftedFrames < 0 && !movingBackwards))
        {
            return;
        }

        double movementDelta = clickedCel.PrecisePosition - lastMovementPrecisePosition;
        if (Math.Abs(movementDelta) < 10)
        {
            return;
        }

        int delta = shiftedFrames;

        if (delta != 0)
        {
            if (!clickedCel.IsSelected)
            {
                SelectKeyFrame(clickedCel);
            }

            dragged = true;
            if (DragAllSelectedKeyFrames(delta))
            {
                dragStartFrame += delta;
            }

            lastMovementPrecisePosition = clickedCel.PrecisePosition;
        }

        PropertyChanged(this, new PropertyChangedEventArgs(nameof(EndFrame)));
    }

    private void KeyFramesPreciseDragged((CelViewModel source, double delta) args)
    {
        if (clickedCel == null) return;

        var allKeyFrames = SelectedKeyFrames;
        foreach (var keyFrame in allKeyFrames)
        {
            if (keyFrame == args.source)
            {
                continue;
            }

            keyFrame.PrecisePosition += args.delta;
        }
    }

    private void ClearSelectedKeyFrames(CelViewModel? keyFrame)
    {
        ClearSelectedKeyFrames();
    }

    private void KeyFramePressed(PointerPressedEventArgs? e)
    {
        e.PreventGestureRecognition(); // Prevents digital pen losing capture when dragging

        if (e.GetMouseButton(this) != MouseButton.Left)
        {
            return;
        }

        shouldShiftSelect = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        shouldClearNextSelection = !shouldShiftSelect && !e.KeyModifiers.HasFlag(KeyModifiers.Control);
        KeyFrame target = null;
        if (e.Source is Control obj)
        {
            if (obj is KeyFrame frame)
                target = frame;
            else if (obj.TemplatedParent is KeyFrame keyFrame) target = keyFrame;
        }

        draggedKeyFrameWasSelected = target is { Item.IsSelected: true };
        e.Pointer.Capture(target);
        clickedCel = target.Item;
        dragStartFrame = MousePosToFrame(e);

        e.Handled = true;
    }

    private void ClearSelectedKeyFrames()
    {
        foreach (var keyFrame in SelectedKeyFrames)
        {
            keyFrame.Document.AnimationDataViewModel.RemoveSelectedKeyFrame(keyFrame.Id);
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
    }

    private void TimelineKeyFramesScrollOnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
        {
            return;
        }

        ScrollOffset = new Vector(scrollViewer.Offset.X, scrollViewer.Offset.Y);
        _timelineSlider.Offset = new Vector(scrollViewer.Offset.X, 0);
        _timelineHeaderScroll!.Offset = new Vector(0, scrollViewer.Offset.Y);
    }


    private void PlayToggleOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton toggleButton)
        {
            return;
        }

        if (toggleButton.IsChecked == true)
        {
            IsPlaying = true;
        }
        else
        {
            IsPlaying = false;
        }
    }

    private void TimelineSliderOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        double newScale = Scale;

        int ticks = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? 1 : 10;

        int towardsFrame = MousePosToFrame(e);

        double delta = e.Delta.Y == 0 ? e.Delta.X : e.Delta.Y;

        if (delta > 0)
        {
            newScale += ticks;
        }
        else if (delta < 0)
        {
            newScale -= ticks;
        }

        newScale = Math.Clamp(newScale, 1, 900);
        Scale = newScale;

        double mouseXInViewport = e.GetPosition(_timelineKeyFramesScroll).X;

        double currentFrameUnderMouse = towardsFrame - 1;
        double newOffsetX = currentFrameUnderMouse * newScale - mouseXInViewport + MinLeftOffset;

        extendingElement.Margin = new Thickness(_timelineKeyFramesScroll.Viewport.Width + newOffsetX * 1.1f, 0, 0, 0);

        Dispatcher.UIThread.Post(
            () =>
            {
                newOffsetX = Math.Clamp(newOffsetX, 0, _timelineKeyFramesScroll.ScrollBarMaximum.X);
                ScrollOffset = new Vector(newOffsetX, 0);
            }, DispatcherPriority.Render);

        e.Handled = true;
    }

    private void ContentOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Grid content)
        {
            return;
        }

        var mouseButton = e.GetMouseButton(content);
        e.PreventGestureRecognition();

        if (mouseButton == MouseButton.Left && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            _selectionRectangle.IsVisible = true;
            _selectionRectangle.Width = 0;
            _selectionRectangle.Height = 0;
        }
        else if (mouseButton == MouseButton.Middle ||
                 (mouseButton == MouseButton.Left && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(content);

            if (_timelineKeyFramesScroll.ScrollBarMaximum.X == ScrollOffset.X)
            {
                extendingElement.Margin = new Thickness(_timelineKeyFramesScroll.Viewport.Width, 0, 0, 0);
            }
        }

        clickPos = e.GetPosition(content);
        e.Handled = true;
    }

    private void ContentOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Source is not Grid content)
        {
            return;
        }

        if (e.GetCurrentPoint(content).Properties.IsLeftButtonPressed && !e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            HandleMoveSelection(e, content);
        }
        else if (e.GetCurrentPoint(content).Properties.IsMiddleButtonPressed ||
                 (e.GetCurrentPoint(content).Properties.IsLeftButtonPressed &&
                  e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            HandleTimelinePan(e, content);
        }
    }

    private void HandleTimelinePan(PointerEventArgs e, Grid content)
    {
        double deltaX = clickPos.X - e.GetPosition(content).X;
        double deltaY = clickPos.Y - e.GetPosition(content).Y;
        double newOffsetX = ScrollOffset.X + deltaX;
        double newOffsetY = ScrollOffset.Y + deltaY;
        newOffsetX = Math.Clamp(newOffsetX, 0, _timelineKeyFramesScroll.ScrollBarMaximum.X);
        newOffsetY = Math.Clamp(newOffsetY, 0, _timelineKeyFramesScroll.ScrollBarMaximum.Y);
        ScrollOffset = new Vector(newOffsetX, newOffsetY);

        extendingElement.Margin += new Thickness(deltaX, 0, 0, 0);
    }

    private void HandleMoveSelection(PointerEventArgs e, Grid content)
    {
        double x = e.GetPosition(content).X;
        double y = e.GetPosition(content).Y;
        double width = x - clickPos.X;
        double height = y - clickPos.Y;
        _selectionRectangle.Width = Math.Abs(width);
        _selectionRectangle.Height = Math.Abs(height);
        Thickness margin = new Thickness(Math.Min(clickPos.X, x), Math.Min(clickPos.Y, y), 0, 0);
        _selectionRectangle.Margin = margin;
        ClearSelectedKeyFrames();

        SelectAllWithinBounds(_selectionRectangle.Bounds);
    }

    private void SelectAllWithinBounds(Rect bounds)
    {
        var frames = _keyFramesHost.ItemsPanelRoot.GetVisualDescendants().OfType<KeyFrame>();
        foreach (var frame in frames)
        {
            var translated = frame.TranslatePoint(new Point(0, 0), _contentGrid);
            Rect frameBounds = new Rect(translated.Value.X, translated.Value.Y, frame.Bounds.Width,
                frame.Bounds.Height);
            if (bounds.Intersects(frameBounds))
            {
                SelectKeyFrame(frame.Item, false);
            }
        }
    }

    private void ContentOnPointerLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (e.Source is not Grid content)
        {
            return;
        }

        Cursor = new Cursor(StandardCursorType.Arrow);
        _selectionRectangle.IsVisible = false;
    }

    private int MousePosToFrame(PointerEventArgs e, bool round = true)
    {
        double x = e.GetPosition(_contentGrid).X;
        return MousePosToFrame(x, round);
    }

    private int MousePosToFrame(double x, bool round = true)
    {
        x -= MinLeftOffset;
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

    private int MousePosToFrameCeil(double x)
    {
        x -= MinLeftOffset;
        int frame;

        frame = (int)Math.Ceiling(x / Scale) + 1;

        frame = Math.Max(1, frame);
        return frame;
    }

    private static void OnKeyFramesChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not Timeline timeline)
        {
            return;
        }

        if (e.OldValue is KeyFrameCollection oldCollection)
        {
            oldCollection.KeyFrameAdded -= timeline.KeyFrames_KeyFrameAdded;
            oldCollection.KeyFrameRemoved -= timeline.KeyFrames_KeyFrameRemoved;
        }

        if (e.NewValue is KeyFrameCollection newCollection)
        {
            newCollection.KeyFrameAdded += timeline.KeyFrames_KeyFrameAdded;
            newCollection.KeyFrameRemoved += timeline.KeyFrames_KeyFrameRemoved;

            foreach (var item in newCollection)
            {
                foreach (var child in item.Children)
                {
                    if (child is CelViewModel cel)
                    {
                        cel.PropertyChanged += timeline.KeyFrameOnPropertyChanged;
                    }
                }
            }
        }

        if (timeline.PropertyChanged != null)
        {
            timeline.PropertyChanged(timeline, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
            timeline.PropertyChanged(timeline, new PropertyChangedEventArgs(nameof(EndFrame)));
        }
    }

    private void KeyFrames_KeyFrameAdded(CelViewModel cel)
    {
        cel.PropertyChanged += KeyFrameOnPropertyChanged;
        if (cel is CelGroupViewModel group)
        {
            group.Children.CollectionChanged += GroupChildren_CollectionChanged;
        }

        PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(EndFrame)));
    }

    private void GroupChildren_CollectionChanged(object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CelViewModel cel in e.NewItems)
            {
                cel.PropertyChanged += KeyFrameOnPropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (CelViewModel cel in e.OldItems)
            {
                cel.PropertyChanged -= KeyFrameOnPropertyChanged;
            }
        }

        PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(EndFrame)));
    }

    private void KeyFrames_KeyFrameRemoved(CelViewModel cel)
    {
        if (SelectedKeyFrames.Contains(cel))
        {
            cel.Document.AnimationDataViewModel.RemoveSelectedKeyFrame(cel.Id);
            cel.PropertyChanged -= KeyFrameOnPropertyChanged;

            if (cel is CelGroupViewModel group)
            {
                group.Children.CollectionChanged -= GroupChildren_CollectionChanged;
            }
        }

        PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
        PropertyChanged(this, new PropertyChangedEventArgs(nameof(EndFrame)));
    }

    private static void OnDefaultEndFrameChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is not Timeline timeline)
        {
            return;
        }

        if (timeline.PropertyChanged != null)
        {
            timeline.PropertyChanged(timeline, new PropertyChangedEventArgs(nameof(EndFrame)));
        }
    }

    private void KeyFrameOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is CelViewModel keyFrame)
        {
            if (e.PropertyName == nameof(CelViewModel.IsSelected))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedKeyFrames)));
            }
            else if (e.PropertyName == nameof(CelViewModel.StartFrameBindable) ||
                     e.PropertyName == nameof(CelViewModel.DurationBindable))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(EndFrame)));
            }
        }
    }
}
