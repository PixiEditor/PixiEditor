using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.Models.Commands.XAML;
using PixiEditor.AvaloniaUI.Views.Overlays;
using PixiEditor.AvaloniaUI.Views.Overlays.LineToolOverlay;
using PixiEditor.AvaloniaUI.Views.Overlays.SelectionOverlay;
using PixiEditor.AvaloniaUI.Views.Overlays.SymmetryOverlay;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.AvaloniaUI.Views.Visuals;

namespace PixiEditor.AvaloniaUI.Views.Main.ViewportControls;

internal class ViewportOverlays
{
    public Viewport Viewport { get; set; }

    private GridLinesOverlay gridLinesOverlayOverlay;
    private SelectionOverlay selectionOverlay;
    private SymmetryOverlay symmetryOverlay;
    private LineToolOverlay lineToolOverlay;
    private TransformOverlay transformOverlay;
    private ReferenceLayerOverlay referenceLayerOverlay;

    public void Init(Viewport viewport)
    {
        Viewport = viewport;
        gridLinesOverlayOverlay = new GridLinesOverlay();
        BindGridLines();

        selectionOverlay = new SelectionOverlay();
        BindSelectionOverlay();

        symmetryOverlay = new SymmetryOverlay();
        BindSymmetryOverlay();

        lineToolOverlay = new LineToolOverlay();
        BindLineToolOverlay();

        transformOverlay = new TransformOverlay();
        BindTransformOverlay();

        referenceLayerOverlay = new ReferenceLayerOverlay();
        BindReferenceLayerOverlay();

        Viewport.ActiveOverlays.Add(gridLinesOverlayOverlay);
        Viewport.ActiveOverlays.Add(referenceLayerOverlay);
        Viewport.ActiveOverlays.Add(selectionOverlay);
        Viewport.ActiveOverlays.Add(symmetryOverlay);
        Viewport.ActiveOverlays.Add(lineToolOverlay);
        Viewport.ActiveOverlays.Add(transformOverlay);
    }

    private void BindReferenceLayerOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport,
            Path = "Document.ReferenceLayerViewModel.IsVisibleBindable",
            Mode = BindingMode.OneWay
        };

        Binding referenceLayerBinding = new()
        {
            Source = Viewport,
            Path = "Document.ReferenceLayerViewModel",
            Mode = BindingMode.OneWay
        };

        Binding referenceShapeBinding = new()
        {
            Source = Viewport,
            Path = "Document.ReferenceLayerViewModel.ReferenceShapeBindable",
            Mode = BindingMode.OneWay
        };

        Binding fadeOutBinding = new()
        {
            Source = Viewport,
            Path = "!Document.ToolsSubViewModel.ColorPickerToolViewModel.PickFromReferenceLayer",
            Mode = BindingMode.OneWay,
        };

        referenceLayerOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.ReferenceLayerProperty, referenceLayerBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.ReferenceShapeProperty, referenceShapeBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.FadeOutProperty, fadeOutBinding);
    }

    private void BindGridLines()
    {
        Binding isVisBinding = new()
        {
            Source = Viewport,
            Path = "GridLinesVisible",
            Mode = BindingMode.OneWay
        };

        Binding binding = new()
        {
            Source = Viewport,
            Path = "Document.Width",
            Mode = BindingMode.OneWay
        };

        gridLinesOverlayOverlay.Bind(GridLinesOverlay.PixelWidthProperty, binding);
        gridLinesOverlayOverlay.Bind(GridLinesOverlay.ColumnsProperty, binding);

        binding = new Binding
        {
            Source = Viewport,
            Path = "Document.Height",
            Mode = BindingMode.OneWay
        };

        gridLinesOverlayOverlay.Bind(GridLinesOverlay.PixelHeightProperty, binding);
        gridLinesOverlayOverlay.Bind(GridLinesOverlay.RowsProperty, binding);
        gridLinesOverlayOverlay.Bind(Visual.IsVisibleProperty, isVisBinding);
    }

    private void BindSelectionOverlay()
    {
        Binding showFillBinding = new()
        {
            Source = Viewport,
            Path = "Document.ToolsSubViewModel.ActiveTool",
            Converter = new IsSelectionToolConverter(),
            Mode = BindingMode.OneWay
        };

        Binding pathBinding = new()
        {
            Source = Viewport,
            Path = "Document.SelectionPathBindable",
            Mode = BindingMode.OneWay
        };

        Binding isVisibleBinding = new()
        {
            Source = Viewport,
            Path = "Document.SelectionPathBindable",
            Mode = BindingMode.OneWay,
            Converter = new NotNullToVisibilityConverter()
        };

        selectionOverlay.Bind(SelectionOverlay.ShowFillProperty, showFillBinding);
        selectionOverlay.Bind(SelectionOverlay.PathProperty, pathBinding);
        selectionOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
    }

    private void BindSymmetryOverlay()
    {
        Binding sizeBinding = new() { Source = Viewport, Path = "Document.SizeBindable", Mode = BindingMode.OneWay };
        Binding isHitTestVisibleBinding = new() {Source = Viewport, Path = "ZoomMode", Converter = new ZoomModeToHitTestVisibleConverter(), Mode = BindingMode.OneWay };
        Binding horizontalAxisVisibleBinding = new() {Source = Viewport, Path = "Document.HorizontalSymmetryAxisEnabledBindable", Mode = BindingMode.OneWay };
        Binding verticalAxisVisibleBinding = new() {Source = Viewport, Path = "Document.VerticalSymmetryAxisEnabledBindable", Mode = BindingMode.OneWay };
        Binding horizontalAxisYBinding = new() {Source = Viewport, Path = "Document.HorizontalSymmetryAxisYBindable", Mode = BindingMode.OneWay };
        Binding verticalAxisXBinding = new() {Source = Viewport, Path = "Document.VerticalSymmetryAxisXBindable", Mode = BindingMode.OneWay };

        symmetryOverlay.Bind(SymmetryOverlay.SizeProperty, sizeBinding);
        symmetryOverlay.Bind(InputElement.IsHitTestVisibleProperty, isHitTestVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.HorizontalAxisVisibleProperty, horizontalAxisVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.VerticalAxisVisibleProperty, verticalAxisVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.HorizontalAxisYProperty, horizontalAxisYBinding);
        symmetryOverlay.Bind(SymmetryOverlay.VerticalAxisXProperty, verticalAxisXBinding);
        symmetryOverlay.DragCommand = (ICommand)new Command("PixiEditor.Document.DragSymmetry") { UseProvided = true }.ProvideValue(null);
        symmetryOverlay.DragEndCommand = (ICommand)new Command("PixiEditor.Document.EndDragSymmetry") { UseProvided = true }.ProvideValue(null);
        symmetryOverlay.DragStartCommand = (ICommand)new Command("PixiEditor.Document.StartDragSymmetry") { UseProvided = true }.ProvideValue(null);
    }

    private void BindLineToolOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport,
            Path = "Document.LineToolOverlayViewModel.IsEnabled",
            Mode = BindingMode.OneWay
        };

        Binding actionCompletedBinding = new()
        {
            Source = Viewport,
            Path = "Document.LineToolOverlayViewModel.ActionCompletedCommand",
            Mode = BindingMode.OneWay
        };

        Binding lineStartBinding = new()
        {
            Source = Viewport,
            Path = "Document.LineToolOverlayViewModel.LineStart",
            Mode = BindingMode.TwoWay
        };

        Binding lineEndBinding = new()
        {
            Source = Viewport,
            Path = "Document.LineToolOverlayViewModel.LineEnd",
            Mode = BindingMode.TwoWay
        };

        lineToolOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        lineToolOverlay.Bind(LineToolOverlay.ActionCompletedProperty, actionCompletedBinding);
        lineToolOverlay.Bind(LineToolOverlay.LineStartProperty, lineStartBinding);
        lineToolOverlay.Bind(LineToolOverlay.LineEndProperty, lineEndBinding);
    }

    private void BindTransformOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.TransformActive",
            Mode = BindingMode.OneWay
        };

        Binding actionCompletedBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.ActionCompletedCommand",
            Mode = BindingMode.OneWay
        };

        Binding cornersBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.Corners",
            Mode = BindingMode.TwoWay
        };

        Binding requestedCornersBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.RequestedCorners",
            Mode = BindingMode.TwoWay
        };

        Binding cornerFreedomBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.CornerFreedom",
            Mode = BindingMode.OneWay
        };

        Binding sideFreedomBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.SideFreedom",
            Mode = BindingMode.OneWay
        };

        Binding lockRotationBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.LockRotation",
            Mode = BindingMode.OneWay
        };

        Binding coverWholeScreenBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.CoverWholeScreen",
            Mode = BindingMode.OneWay
        };

        Binding snapToAnglesBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.SnapToAngles",
            Mode = BindingMode.OneWay
        };

        Binding internalStateBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.InternalState",
            Mode = BindingMode.TwoWay
        };

        Binding zoomboxAngleBinding = new()
        {
            Source = Viewport,
            Path = "Zoombox.Angle",
            Mode = BindingMode.OneWay
        };

        transformOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        transformOverlay.Bind(TransformOverlay.ActionCompletedProperty, actionCompletedBinding);
        transformOverlay.Bind(TransformOverlay.CornersProperty, cornersBinding);
        transformOverlay.Bind(TransformOverlay.RequestedCornersProperty, requestedCornersBinding);
        transformOverlay.Bind(TransformOverlay.CornerFreedomProperty, cornerFreedomBinding);
        transformOverlay.Bind(TransformOverlay.SideFreedomProperty, sideFreedomBinding);
        transformOverlay.Bind(TransformOverlay.LockRotationProperty, lockRotationBinding);
        transformOverlay.Bind(TransformOverlay.CoverWholeScreenProperty, coverWholeScreenBinding);
        transformOverlay.Bind(TransformOverlay.SnapToAnglesProperty, snapToAnglesBinding);
        transformOverlay.Bind(TransformOverlay.InternalStateProperty, internalStateBinding);
        transformOverlay.Bind(TransformOverlay.ZoomboxAngleProperty, zoomboxAngleBinding);
    }
}
