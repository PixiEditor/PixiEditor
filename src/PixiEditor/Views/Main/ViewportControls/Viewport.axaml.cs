using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Skia;
using Avalonia.VisualTree;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers;
using PixiEditor.ViewModels;
using PixiEditor.Views.Visuals;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Position;
using Drawie.Numerics;
using Drawie.Skia;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.UI.Common.Behaviors;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Views.Overlays;
using PixiEditor.Views.Rendering;
using PixiEditor.Zoombox;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Main.ViewportControls;

#nullable enable
internal partial class Viewport : UserControl, INotifyPropertyChanged
{
    //TODO: IDK where to write this, but on close zoom level, when I drag line handle, it doesn't update the canvas
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly StyledProperty<bool> FlipXProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(FlipX), false);

    public static readonly StyledProperty<bool> FlipYProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(FlipY), false);

    public static readonly StyledProperty<ZoomboxMode> ZoomModeProperty =
        AvaloniaProperty.Register<Viewport, ZoomboxMode>(nameof(ZoomMode), ZoomboxMode.Normal);

    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<Viewport, DocumentViewModel>(nameof(Document), null);

    public static readonly StyledProperty<ICommand> MouseDownCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseDownCommand), null);

    public static readonly StyledProperty<ICommand> MouseMoveCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseMoveCommand), null);

    public static readonly StyledProperty<ICommand> MouseUpCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MouseUpCommand), null);

    public static readonly StyledProperty<bool> DelayedProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(Delayed), false);

    public static readonly StyledProperty<bool> GridLinesVisibleProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(GridLinesVisible), false);

    public static readonly StyledProperty<double> GridLinesXSizeProperty =
        AvaloniaProperty.Register<Viewport, double>(nameof(GridLinesXSize), 1.0);

    public static readonly StyledProperty<double> GridLinesYSizeProperty =
        AvaloniaProperty.Register<Viewport, double>(nameof(GridLinesYSize), 1.0);

    public static readonly StyledProperty<bool> ZoomOutOnClickProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(ZoomOutOnClick), false);

    public static readonly StyledProperty<ExecutionTrigger<double>> ZoomViewportTriggerProperty =
        AvaloniaProperty.Register<Viewport, ExecutionTrigger<double>>(nameof(ZoomViewportTrigger), null);

    public static readonly StyledProperty<ExecutionTrigger<VecI>> CenterViewportTriggerProperty =
        AvaloniaProperty.Register<Viewport, ExecutionTrigger<VecI>>(nameof(CenterViewportTrigger), null);

    public static readonly StyledProperty<bool> UseTouchGesturesProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(UseTouchGestures), false);

    public static readonly StyledProperty<ICommand> StylusButtonDownCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusButtonDownCommand), null);

    public static readonly StyledProperty<ICommand> StylusButtonUpCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusButtonUpCommand), null);

    public static readonly StyledProperty<ICommand> StylusGestureCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusGestureCommand), null);

    public static readonly StyledProperty<ICommand> StylusOutOfRangeCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(StylusOutOfRangeCommand), null);

    public static readonly StyledProperty<ICommand> MiddleMouseClickedCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(nameof(MiddleMouseClickedCommand), null);

    public static readonly StyledProperty<ICommand> MouseWheelCommandProperty =
        AvaloniaProperty.Register<Viewport, ICommand>(
            nameof(MouseWheelCommand));

    public static readonly StyledProperty<ViewportColorChannels> ChannelsProperty =
        AvaloniaProperty.Register<Viewport, ViewportColorChannels>(
            nameof(Channels));

    public static readonly StyledProperty<bool> IsOverCanvasProperty = AvaloniaProperty.Register<Viewport, bool>(
        nameof(IsOverCanvas));

    public static readonly StyledProperty<SnappingViewModel> SnappingViewModelProperty =
        AvaloniaProperty.Register<Viewport, SnappingViewModel>(
            nameof(SnappingViewModel));

    public static readonly StyledProperty<bool> HighResPreviewProperty =
        AvaloniaProperty.Register<Viewport, bool>(nameof(HighResPreview), true);

    public static readonly StyledProperty<bool> AutoBackgroundScaleProperty = AvaloniaProperty.Register<Viewport, bool>(
        nameof(AutoBackgroundScale), true);

    public static readonly StyledProperty<double> CustomBackgroundScaleXProperty =
        AvaloniaProperty.Register<Viewport, double>(
            nameof(CustomBackgroundScaleX));

    public static readonly StyledProperty<double> CustomBackgroundScaleYProperty =
        AvaloniaProperty.Register<Viewport, double>(
            nameof(CustomBackgroundScaleY));

    public static readonly StyledProperty<Bitmap> BackgroundBitmapProperty =
        AvaloniaProperty.Register<Viewport, Bitmap>(
            nameof(BackgroundBitmap));

    public Bitmap BackgroundBitmap
    {
        get => GetValue(BackgroundBitmapProperty);
        set => SetValue(BackgroundBitmapProperty, value);
    }

    public double CustomBackgroundScaleY
    {
        get => GetValue(CustomBackgroundScaleYProperty);
        set => SetValue(CustomBackgroundScaleYProperty, value);
    }

    public double CustomBackgroundScaleX
    {
        get => GetValue(CustomBackgroundScaleXProperty);
        set => SetValue(CustomBackgroundScaleXProperty, value);
    }

    public bool AutoBackgroundScale
    {
        get => GetValue(AutoBackgroundScaleProperty);
        set => SetValue(AutoBackgroundScaleProperty, value);
    }

    public SnappingViewModel SnappingViewModel
    {
        get => GetValue(SnappingViewModelProperty);
        set => SetValue(SnappingViewModelProperty, value);
    }

    public bool IsOverCanvas
    {
        get => GetValue(IsOverCanvasProperty);
        set => SetValue(IsOverCanvasProperty, value);
    }

    public ICommand? MiddleMouseClickedCommand
    {
        get => (ICommand?)GetValue(MiddleMouseClickedCommandProperty);
        set => SetValue(MiddleMouseClickedCommandProperty, value);
    }

    public ICommand? StylusOutOfRangeCommand
    {
        get => (ICommand?)GetValue(StylusOutOfRangeCommandProperty);
        set => SetValue(StylusOutOfRangeCommandProperty, value);
    }

    public ICommand? StylusGestureCommand
    {
        get => (ICommand?)GetValue(StylusGestureCommandProperty);
        set => SetValue(StylusGestureCommandProperty, value);
    }

    public ICommand? StylusButtonUpCommand
    {
        get => (ICommand?)GetValue(StylusButtonUpCommandProperty);
        set => SetValue(StylusButtonUpCommandProperty, value);
    }

    public ICommand? StylusButtonDownCommand
    {
        get => (ICommand?)GetValue(StylusButtonDownCommandProperty);
        set => SetValue(StylusButtonDownCommandProperty, value);
    }

    public ICommand MouseWheelCommand
    {
        get => GetValue(MouseWheelCommandProperty);
        set => SetValue(MouseWheelCommandProperty, value);
    }

    public bool UseTouchGestures
    {
        get => (bool)GetValue(UseTouchGesturesProperty);
        set => SetValue(UseTouchGesturesProperty, value);
    }

    public ExecutionTrigger<VecI>? CenterViewportTrigger
    {
        get => (ExecutionTrigger<VecI>)GetValue(CenterViewportTriggerProperty);
        set => SetValue(CenterViewportTriggerProperty, value);
    }

    public ExecutionTrigger<double>? ZoomViewportTrigger
    {
        get => (ExecutionTrigger<double>)GetValue(ZoomViewportTriggerProperty);
        set => SetValue(ZoomViewportTriggerProperty, value);
    }

    public bool ZoomOutOnClick
    {
        get => (bool)GetValue(ZoomOutOnClickProperty);
        set => SetValue(ZoomOutOnClickProperty, value);
    }

    public bool GridLinesVisible
    {
        get => (bool)GetValue(GridLinesVisibleProperty);
        set => SetValue(GridLinesVisibleProperty, value);
    }

    public double GridLinesXSize
    {
        get => (double)GetValue(GridLinesXSizeProperty);
        set => SetValue(GridLinesXSizeProperty, value);
    }

    public double GridLinesYSize
    {
        get => (double)GetValue(GridLinesYSizeProperty);
        set => SetValue(GridLinesYSizeProperty, value);
    }

    public bool Delayed
    {
        get => (bool)GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }

    public ICommand? MouseDownCommand
    {
        get => (ICommand?)GetValue(MouseDownCommandProperty);
        set => SetValue(MouseDownCommandProperty, value);
    }

    public ICommand? MouseMoveCommand
    {
        get => (ICommand?)GetValue(MouseMoveCommandProperty);
        set => SetValue(MouseMoveCommandProperty, value);
    }

    public ICommand? MouseUpCommand
    {
        get => (ICommand?)GetValue(MouseUpCommandProperty);
        set => SetValue(MouseUpCommandProperty, value);
    }


    public DocumentViewModel? Document
    {
        get => (DocumentViewModel)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public ZoomboxMode ZoomMode
    {
        get => (ZoomboxMode)GetValue(ZoomModeProperty);
        set => SetValue(ZoomModeProperty, value);
    }

    public bool FlipX
    {
        get => (bool)GetValue(FlipXProperty);
        set => SetValue(FlipXProperty, value);
    }

    public bool FlipY
    {
        get => (bool)GetValue(FlipYProperty);
        set => SetValue(FlipYProperty, value);
    }

    public static readonly StyledProperty<bool> HudVisibleProperty = AvaloniaProperty.Register<Viewport, bool>(
        nameof(HudVisible), true);

    public bool HudVisible
    {
        get => GetValue(HudVisibleProperty);
        set => SetValue(HudVisibleProperty, value);
    }

    public ViewportColorChannels Channels
    {
        get => GetValue(ChannelsProperty);
        set => SetValue(ChannelsProperty, value);
    }

    private double angleRadians = 0;

    public double AngleRadians
    {
        get => angleRadians;
        set
        {
            angleRadians = value;
            PropertyChanged?.Invoke(this, new(nameof(AngleRadians)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD center = new(32, 32);

    public VecD Center
    {
        get => center;
        set
        {
            center = value;
            PropertyChanged?.Invoke(this, new(nameof(Center)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD realDimensions = new(double.MaxValue, double.MaxValue);

    public VecD RealDimensions
    {
        get => realDimensions;
        set
        {
            ChunkResolution oldRes = CalculateResolution();
            realDimensions = value;
            ChunkResolution newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(RealDimensions)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());
        }
    }

    private VecD dimensions = new(64, 64);

    public VecD Dimensions
    {
        get => dimensions;
        set
        {
            ChunkResolution oldRes = CalculateResolution();
            dimensions = value;
            ChunkResolution newRes = CalculateResolution();

            PropertyChanged?.Invoke(this, new(nameof(Dimensions)));
            Document?.Operations.AddOrUpdateViewport(GetLocation());
        }
    }

    public static readonly StyledProperty<ObservableCollection<string>> AvailableRenderOutputsProperty =
        AvaloniaProperty.Register<Viewport, ObservableCollection<string>>(nameof(AvailableRenderOutputs));

    public static readonly StyledProperty<string> ViewportRenderOutputProperty =
        AvaloniaProperty.Register<Viewport, string>(
            nameof(ViewportRenderOutput), "DEFAULT");

    public string ViewportRenderOutput
    {
        get => GetValue(ViewportRenderOutputProperty);
        set => SetValue(ViewportRenderOutputProperty, value);
    }

    public static readonly StyledProperty<Func<EditorData>> EditorDataFuncProperty = AvaloniaProperty.Register<Viewport, Func<EditorData>>("EditorDataFunc");

    public ObservableCollection<Overlay> ActiveOverlays { get; } = new();

    public Guid GuidValue { get; } = Guid.NewGuid();

    private MouseUpdateController? mouseUpdateController;
    private ViewportOverlays builtInOverlays = new();

    public static readonly StyledProperty<int> MaxBilinearSamplingSizeProperty
        = AvaloniaProperty.Register<Viewport, int>("MaxBilinearSamplingSize", 4096);

    public static readonly StyledProperty<bool> SnappingEnabledProperty =
        AvaloniaProperty.Register<Viewport, bool>("SnappingEnabled");

    static Viewport()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChange);
        ViewportRenderOutputProperty.Changed.Subscribe(e =>
        {
            Viewport? viewport = (Viewport)e.Sender;
            viewport.Document?.Operations.AddOrUpdateViewport(viewport.GetLocation());
        });
        ZoomViewportTriggerProperty.Changed.Subscribe(ZoomViewportTriggerChanged);
        CenterViewportTriggerProperty.Changed.Subscribe(CenterViewportTriggerChanged);
        HighResPreviewProperty.Changed.Subscribe(OnHighResPreviewChanged);
    }

    public Viewport()
    {
        InitializeComponent();

        builtInOverlays.Init(this);
        Scene!.Loaded += OnImageLoaded;
        Scene.SizeChanged += OnMainImageSizeChanged;
        Loaded += OnLoad;
        Unloaded += OnUnload;
        Scene.AttachedToVisualTree += OnAttachedToVisualTree;

        //TODO: It's weird that I had to do it this way, right click didn't raise Image_MouseUp otherwise.
        viewportGrid.AddHandler(PointerReleasedEvent, Image_MouseUp, RoutingStrategies.Tunnel);
        viewportGrid.AddHandler(PointerPressedEvent, Image_MouseDown, RoutingStrategies.Bubble);
        viewportGrid.AddHandler(PointerWheelChangedEvent, Image_MouseWheel, RoutingStrategies.Tunnel);

        Scene.PointerExited += (sender, args) => IsOverCanvas = false;
        Scene.PointerEntered += (sender, args) => IsOverCanvas = true;
        Scene.ScaleChanged += OnScaleChanged;
    }

    private void OnScaleChanged(double newScale)
    {
        SnappingViewModel.SnappingController.SnapDistance = SnappingController.DefaultSnapDistance / newScale;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        TextBoxFocusBehavior.FallbackFocusElement.Focus();
    }

    public Scene Scene => scene;

    public bool HighResPreview
    {
        get { return (bool)GetValue(HighResPreviewProperty); }
        set { SetValue(HighResPreviewProperty, value); }
    }

    public bool SnappingEnabled
    {
        get { return (bool)GetValue(SnappingEnabledProperty); }
        set { SetValue(SnappingEnabledProperty, value); }
    }

    public ObservableCollection<string> AvailableRenderOutputs
    {
        get { return (ObservableCollection<string>)GetValue(AvailableRenderOutputsProperty); }
        set { SetValue(AvailableRenderOutputsProperty, value); }
    }

    public int MaxBilinearSamplingSize
    {
        get { return (int)GetValue(MaxBilinearSamplingSizeProperty); }
        set { SetValue(MaxBilinearSamplingSizeProperty, value); }
    }

    public Func<EditorData> EditorDataFunc
    {
        get { return (Func<EditorData>)GetValue(EditorDataFuncProperty); }
        set { SetValue(EditorDataFuncProperty, value); }
    }

    private void ForceRefreshFinalImage()
    {
        Scene.InvalidateVisual();
    }

    private void OnUnload(object? sender, RoutedEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
        mouseUpdateController?.Dispose();
    }

    private void OnLoad(object? sender, RoutedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
        mouseUpdateController = new MouseUpdateController(this, Image_MouseMove);
    }

    private static void OnDocumentChange(AvaloniaPropertyChangedEventArgs<DocumentViewModel> e)
    {
        Viewport? viewport = (Viewport)e.Sender;

        DocumentViewModel? oldDoc = e.OldValue.Value;

        if (oldDoc != null)
        {
            oldDoc.SizeChanged -= viewport.OnDocumentSizeChanged;
        }

        DocumentViewModel? newDoc = e.NewValue.Value;

        if (newDoc != null)
        {
            newDoc.SizeChanged += viewport.OnDocumentSizeChanged;
        }

        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private void OnDocumentSizeChanged(object? sender, DocumentSizeChangedEventArgs documentSizeChangedEventArgs)
    {
        scene.CenterContent(Document.GetRenderOutputSize(ViewportRenderOutput));
    }

    private ChunkResolution CalculateResolution()
    {
        VecD densityVec = Dimensions.Divide(RealDimensions);
        double density = Math.Min(densityVec.X, densityVec.Y);
        if (density > 8.01)
            return ChunkResolution.Eighth;
        else if (density > 4.01)
            return ChunkResolution.Quarter;
        else if (density > 2.01)
            return ChunkResolution.Half;
        return ChunkResolution.Full;
    }

    private ViewportInfo GetLocation()
    {
        return new(AngleRadians, Center, RealDimensions,
            new ViewportData(Scene.CalculateTransformMatrix().ToSKMatrix().ToMatrix3X3(), Scene.Pan, Scene.Scale, FlipX, FlipY),
            Scene.LastPointerInfo,
            Scene.LastKeyboardInfo,
            EditorDataFunc(),
            CalculateVisibleRegion(),
            ViewportRenderOutput, Scene.CalculateSampling(), Dimensions, CalculateResolution(), GuidValue, Delayed,
            true, ForceRefreshFinalImage);
    }

    private void Image_MouseDown(object? sender, PointerPressedEventArgs e)
    {
        if (Document is null || e.Source != Scene)
            return;

        Scene.Focus(NavigationMethod.Pointer);

        bool isMiddle = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;
        HandleMiddleMouse(isMiddle);

        if (MouseDownCommand is null)
            return;

        MouseButton mouseButton = e.GetMouseButton(this);

        var pos = e.GetPosition(Scene);
        VecD scenePos = Scene.ToZoomboxSpace(new VecD(pos.X, pos.Y));
        MouseOnCanvasEventArgs? parameter =
            new MouseOnCanvasEventArgs(mouseButton, e.Pointer.Type, scenePos, e.KeyModifiers, e.ClickCount,
                e.GetCurrentPoint(this).Properties, Scene.Scale);

        if (MouseDownCommand.CanExecute(parameter))
            MouseDownCommand.Execute(parameter);
    }

    private void Image_MouseMove(PointerEventArgs e)
    {
        if (MouseMoveCommand is null)
            return;
        Point pos = e.GetPosition(Scene);
        VecD conv = Scene.ToZoomboxSpace(new VecD(pos.X, pos.Y));

        MouseButton mouseButton = e.GetMouseButton(this);

        MouseOnCanvasEventArgs parameter = new(mouseButton, e.Pointer.Type, conv, e.KeyModifiers, 0, e.GetCurrentPoint(this).Properties, Scene.Scale);

        if (MouseMoveCommand.CanExecute(parameter))
            MouseMoveCommand.Execute(parameter);
    }

    private void Image_MouseUp(object? sender, PointerReleasedEventArgs e)
    {
        if (MouseUpCommand is null)
            return;

        Point pos = e.GetPosition(Scene);
        VecD conv = Scene.ToZoomboxSpace(new VecD(pos.X, pos.Y));
        MouseOnCanvasEventArgs parameter = new(e.InitialPressMouseButton, e.Pointer.Type, conv, e.KeyModifiers, 0, e.GetCurrentPoint(this).Properties, Scene.Scale);
        if (MouseUpCommand.CanExecute(parameter))
            MouseUpCommand.Execute(parameter);
    }

    private void Image_MouseWheel(object? sender, PointerWheelEventArgs e)
    {
        if (MouseWheelCommand is null)
            return;

        VecD delta = new VecD(e.Delta.X, e.Delta.Y);
        Point pos = e.GetPosition(Scene);
        VecD posOnCanvas = Scene.ToZoomboxSpace(new VecD(pos.X, pos.Y));
        ScrollOnCanvasEventArgs args = new ScrollOnCanvasEventArgs(delta, posOnCanvas, e.KeyModifiers);
        if (MouseWheelCommand.CanExecute(args))
            MouseWheelCommand.Execute(args);

        e.Handled = args.Handled;
    }

    private void CenterZoomboxContent(object? sender, VecI args)
    {
        scene.CenterContent(args);
    }

    private void ZoomZoomboxContent(object? sender, double delta)
    {
        scene.ZoomIntoCenter(delta);
    }

    private void OnImageLoaded(object sender, EventArgs e)
    {
        scene.CenterContent();
    }

    private void OnMainImageSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (scene.Dimensions is { X: 0, Y: 0 }) return;
        scene.CenterContent();
        scene.ZoomIntoCenter(-1);
        scene.ZoomIntoCenter(1); // a bit hacky, but it resets brush overlay properly
    }

    private void ResetViewportClicked(object sender, RoutedEventArgs e)
    {
        scene.AngleRadians = 0;
        scene.CenterContent(Document.GetRenderOutputSize(ViewportRenderOutput));
    }

    private RectI? CalculateVisibleRegion()
    {
        if (Document is null) return null;
        VecD viewportDimensions = scene.RealDimensions;
        var transform = scene.CanvasTransform.Value.ToSKMatrix().ToMatrix3X3();
        var docSize = Document.GetRenderOutputSize(ViewportRenderOutput);
        if (docSize.X == 0 || docSize.Y == 0) return null;

        VecD topLeft = new(0, 0);
        VecD topRight = new(viewportDimensions.X, 0);
        VecD bottomLeft = new(0, viewportDimensions.Y);
        VecD bottomRight = new(viewportDimensions.X, viewportDimensions.Y);
        topLeft = transform.Invert().MapPoint(topLeft);
        topRight = transform.Invert().MapPoint(topRight);
        bottomLeft = transform.Invert().MapPoint(bottomLeft);
        bottomRight = transform.Invert().MapPoint(bottomRight);

        double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
        double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
        double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
        double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));
        RectD visibleRect = new(minX, minY, maxX - minX, maxY - minY);
        visibleRect = visibleRect.Intersect(new RectD(0, 0, docSize.X, docSize.Y));
        return (RectI)visibleRect.RoundOutwards();
    }

    private static void CenterViewportTriggerChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<VecI>> e)
    {
        Viewport? viewport = (Viewport)e.Sender;
        if (e.OldValue.Value != null)
            e.OldValue.Value.Triggered -= viewport.CenterZoomboxContent;
        if (e.NewValue.Value != null)
            e.NewValue.Value.Triggered += viewport.CenterZoomboxContent;
    }

    private static void ZoomViewportTriggerChanged(AvaloniaPropertyChangedEventArgs<ExecutionTrigger<double>> e)
    {
        Viewport? viewport = (Viewport)e.Sender;
        if (e.OldValue.Value != null)
            e.OldValue.Value.Triggered -= viewport.ZoomZoomboxContent;
        if (e.NewValue.Value != null)
            e.NewValue.Value.Triggered += viewport.ZoomZoomboxContent;
    }

    private void HandleMiddleMouse(bool isMiddle)
    {
        if (MiddleMouseClickedCommand is null)
            return;
        if (isMiddle && MiddleMouseClickedCommand.CanExecute(null))
            MiddleMouseClickedCommand.Execute(null);
    }

    private static void OnHighResPreviewChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        Viewport? viewport = (Viewport)e.Sender;
        viewport.Document?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private void MenuItem_OnClick(object? sender, PointerReleasedEventArgs e)
    {
        Scene?.ContextFlyout?.Hide();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Scene?.ContextFlyout?.Hide();
    }

    private void Scene_OnContextMenuOpening(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetMouseButton(this) != MouseButton.Right) return;

        ViewportWindowViewModel vm = ((ViewportWindowViewModel)DataContext);
        var tools = vm.Owner.Owner.ToolsSubViewModel;

        /*
        var superSpecialBrightnessTool = tools.RightClickMode == RightClickMode.SecondaryColor &&
                                         tools.ActiveTool is BrightnessToolViewModel;
        */
        var superSpecialColorPicker =
            tools.RightClickMode == RightClickMode.Erase && tools.ActiveTool is ColorPickerToolViewModel;

        if (/*superSpecialBrightnessTool || */superSpecialColorPicker)
        {
            return;
        }

        var useContextMenu = vm.Owner.Owner.ToolsSubViewModel.RightClickMode == RightClickMode.ContextMenu;
        var usesErase = tools is { RightClickMode: RightClickMode.Erase, ActiveTool.IsErasable: true };
        bool usesColorPicker = vm.Owner.Owner.ToolsSubViewModel.RightClickMode == RightClickMode.ColorPicker;
        var usesSecondaryColor = tools is { RightClickMode: RightClickMode.SecondaryColor, ActiveTool.UsesColor: true };

        if (!useContextMenu && (usesErase || usesSecondaryColor || usesColorPicker))
        {
            return;
        }

        Scene?.ContextFlyout?.ShowAt(Scene);
        e.Handled = true;
    }

    private void RenderOutput_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source is ComboBox comboBox)
        {
            if (!comboBox.IsAttachedToVisualTree()) return;

            if (e.AddedItems.Count > 0)
            {
                ViewportRenderOutput = (string)e.AddedItems[0];
            }
        }
    }
}
