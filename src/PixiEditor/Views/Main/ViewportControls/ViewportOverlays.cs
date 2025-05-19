using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.Views.Visuals;
using PixiEditor.Helpers.Converters;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.Views.Overlays;
using PixiEditor.Views.Overlays.BrushShapeOverlay;
using PixiEditor.Views.Overlays.LineToolOverlay;
using PixiEditor.Views.Overlays.PathOverlay;
using PixiEditor.Views.Overlays.Pointers;
using PixiEditor.Views.Overlays.SelectionOverlay;
using PixiEditor.Views.Overlays.SymmetryOverlay;
using PixiEditor.Views.Overlays.TextOverlay;
using PixiEditor.Views.Overlays.TransformOverlay;

namespace PixiEditor.Views.Main.ViewportControls;

internal class ViewportOverlays
{
    public Viewport Viewport { get; set; }

    private GridLinesOverlay gridLinesOverlay;
    private SelectionOverlay selectionOverlay;
    private SymmetryOverlay symmetryOverlay;
    private LineToolOverlay lineToolOverlay;
    private TransformOverlay transformOverlay;
    private ReferenceLayerOverlay referenceLayerOverlay;
    private SnappingOverlay snappingOverlay;
    private BrushShapeOverlay brushShapeOverlay;
    private VectorPathOverlay vectorPathOverlay;
    private TextOverlay textOverlay;

    public void Init(Viewport viewport)
    {
        Viewport = viewport;
        gridLinesOverlay = new GridLinesOverlay();
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

        snappingOverlay = new SnappingOverlay();
        BindSnappingOverlay();

        brushShapeOverlay = new BrushShapeOverlay();
        BindMouseOverlayPointer();

        vectorPathOverlay = new VectorPathOverlay();
        vectorPathOverlay.IsVisible = false;
        BindVectorPathOverlay();

        textOverlay = new TextOverlay();
        BindTextOverlay();

        Binding suppressOverlayEventsBinding = new()
        {
            Source = Viewport,
            Path = "Document.OverlayEventsSuppressed",
            Mode = BindingMode.OneWay
        };

        Viewport.ActiveOverlays.Add(gridLinesOverlay);
        Viewport.ActiveOverlays.Add(referenceLayerOverlay);
        Viewport.ActiveOverlays.Add(selectionOverlay);
        Viewport.ActiveOverlays.Add(symmetryOverlay);
        Viewport.ActiveOverlays.Add(lineToolOverlay);
        Viewport.ActiveOverlays.Add(transformOverlay);
        Viewport.ActiveOverlays.Add(vectorPathOverlay);
        Viewport.ActiveOverlays.Add(snappingOverlay);
        Viewport.ActiveOverlays.Add(brushShapeOverlay);
        Viewport.ActiveOverlays.Add(textOverlay);

        foreach (var overlay in Viewport.ActiveOverlays)
        {
            overlay.Bind(Overlay.SuppressEventsProperty, suppressOverlayEventsBinding);
        }
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
            Source = Viewport, Path = "Document.ReferenceLayerViewModel", Mode = BindingMode.OneWay
        };

        Binding referenceShapeBinding = new()
        {
            Source = Viewport,
            Path = "Document.ReferenceLayerViewModel.ReferenceShapeBindable",
            Mode = BindingMode.OneWay
        };

        Binding fadeOutBinding = new()
        {
            Source = ViewModelMain.Current.ToolsSubViewModel,
            Path = "!ActiveTool.PickFromReferenceLayer",
            Mode = BindingMode.OneWay,
        };

        referenceLayerOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.ReferenceLayerProperty, referenceLayerBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.ReferenceShapeProperty, referenceShapeBinding);
        referenceLayerOverlay.Bind(ReferenceLayerOverlay.FadeOutProperty, fadeOutBinding);
    }

    private void BindGridLines()
    {
        Binding isVisBinding = new() { Source = Viewport, Path = "GridLinesVisible", Mode = BindingMode.OneWay };
        gridLinesOverlay.Bind(Visual.IsVisibleProperty, isVisBinding);

        Binding xBinding = new() { Source = Viewport, Path = "GridLinesXSize", Mode = BindingMode.OneWay };
        gridLinesOverlay.Bind(GridLinesOverlay.GridXSizeProperty, xBinding);
        Binding yBinding = new() { Source = Viewport, Path = "GridLinesYSize", Mode = BindingMode.OneWay };
        gridLinesOverlay.Bind(GridLinesOverlay.GridYSizeProperty, yBinding);
    }

    private void BindSelectionOverlay()
    {
        Binding showFillBinding = new()
        {
            Source = ViewModelMain.Current,
            Path = "ToolsSubViewModel.ActiveTool",
            Converter = new IsSelectionToolConverter(),
            Mode = BindingMode.OneWay
        };

        Binding pathBinding = new()
        {
            Source = Viewport, Path = "Document.SelectionPathBindable", Mode = BindingMode.OneWay
        };

        Binding isVisibleBinding = new()
        {
            Source = Viewport,
            Path = "Document.SelectionPathBindable",
            Mode = BindingMode.OneWay,
            Converter = new VectorPathToVisibleConverter()
        };

        selectionOverlay.Bind(SelectionOverlay.ShowFillProperty, showFillBinding);
        selectionOverlay.Bind(SelectionOverlay.PathProperty, pathBinding);
        selectionOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
    }

    private void BindSymmetryOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.AnySymmetryAxisEnabledBindable", Mode = BindingMode.OneWay
        };
        Binding isHitTestVisibleBinding = new()
        {
            Source = Viewport,
            Path = "ZoomMode",
            Converter = new ZoomModeToHitTestVisibleConverter(),
            Mode = BindingMode.OneWay
        };
        Binding horizontalAxisVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.HorizontalSymmetryAxisEnabledBindable", Mode = BindingMode.OneWay
        };
        Binding verticalAxisVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.VerticalSymmetryAxisEnabledBindable", Mode = BindingMode.OneWay
        };
        Binding horizontalAxisYBinding = new()
        {
            Source = Viewport, Path = "Document.HorizontalSymmetryAxisYBindable", Mode = BindingMode.OneWay
        };
        Binding verticalAxisXBinding = new()
        {
            Source = Viewport, Path = "Document.VerticalSymmetryAxisXBindable", Mode = BindingMode.OneWay
        };

        symmetryOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        symmetryOverlay.Bind(InputElement.IsHitTestVisibleProperty, isHitTestVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.HorizontalAxisVisibleProperty, horizontalAxisVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.VerticalAxisVisibleProperty, verticalAxisVisibleBinding);
        symmetryOverlay.Bind(SymmetryOverlay.HorizontalAxisYProperty, horizontalAxisYBinding);
        symmetryOverlay.Bind(SymmetryOverlay.VerticalAxisXProperty, verticalAxisXBinding);
        symmetryOverlay.DragCommand =
            (ICommand)new Command("PixiEditor.Document.DragSymmetry") { UseProvided = true }.ProvideValue(null);
        symmetryOverlay.DragEndCommand =
            (ICommand)new Command("PixiEditor.Document.EndDragSymmetry") { UseProvided = true }.ProvideValue(null);
        symmetryOverlay.DragStartCommand =
            (ICommand)new Command("PixiEditor.Document.StartDragSymmetry") { UseProvided = true }.ProvideValue(null);
    }

    private void BindLineToolOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.IsEnabled", Mode = BindingMode.OneWay
        };

        Binding snappingBinding = new()
        {
            Source = Viewport, Path = "Document.SnappingViewModel.SnappingController", Mode = BindingMode.OneWay
        };

        Binding actionCompletedBinding = new()
        {
            Source = Viewport,
            Path = "Document.LineToolOverlayViewModel.ActionCompletedCommand",
            Mode = BindingMode.OneWay
        };

        Binding lineStartBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.LineStart", Mode = BindingMode.TwoWay
        };

        Binding lineEndBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.LineEnd", Mode = BindingMode.TwoWay
        };

        Binding showHandlesBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.ShowHandles", Mode = BindingMode.TwoWay
        };

        Binding isSizeBoxEnabledBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.IsSizeBoxEnabled", Mode = BindingMode.TwoWay
        };

        Binding addToUndoCommandBinding = new()
        {
            Source = Viewport, Path = "Document.LineToolOverlayViewModel.AddToUndoCommand", Mode = BindingMode.OneWay
        };

        lineToolOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        lineToolOverlay.Bind(LineToolOverlay.SnappingControllerProperty, snappingBinding);
        lineToolOverlay.Bind(LineToolOverlay.ActionCompletedProperty, actionCompletedBinding);
        lineToolOverlay.Bind(LineToolOverlay.LineStartProperty, lineStartBinding);
        lineToolOverlay.Bind(LineToolOverlay.LineEndProperty, lineEndBinding);
        lineToolOverlay.Bind(LineToolOverlay.ShowHandlesProperty, showHandlesBinding);
        lineToolOverlay.Bind(LineToolOverlay.IsSizeBoxEnabledProperty, isSizeBoxEnabledBinding);
        lineToolOverlay.Bind(LineToolOverlay.ShowHandlesProperty, showHandlesBinding);
        lineToolOverlay.Bind(LineToolOverlay.IsSizeBoxEnabledProperty, isSizeBoxEnabledBinding);
        lineToolOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        lineToolOverlay.Bind(LineToolOverlay.AddToUndoCommandProperty, addToUndoCommandBinding);
    }

    private void BindTransformOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.TransformActive", Mode = BindingMode.OneWay
        };

        Binding snappingBinding = new()
        {
            Source = Viewport, Path = "Document.SnappingViewModel.SnappingController", Mode = BindingMode.OneWay
        };

        Binding actionCompletedBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.ActionCompletedCommand",
            Mode = BindingMode.OneWay
        };

        Binding cornersBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.Corners", Mode = BindingMode.TwoWay
        };

        Binding requestedCornersBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.RequestCornersExecutor",
        };

        Binding cornerFreedomBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.CornerFreedom", Mode = BindingMode.OneWay
        };

        Binding sideFreedomBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.SideFreedom", Mode = BindingMode.OneWay
        };

        Binding lockRotationBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.LockRotation", Mode = BindingMode.OneWay
        };

        Binding coverWholeScreenBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.CoverWholeScreen", Mode = BindingMode.OneWay
        };

        Binding snapToAnglesBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.SnapToAngles", Mode = BindingMode.OneWay
        };

        Binding internalStateBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.InternalState", Mode = BindingMode.TwoWay
        };

        Binding passThroughPointerPressedBinding = new()
        {
            Source = Viewport,
            Path = "Document.TransformViewModel.PassThroughPointerPressedCommand",
            Mode = BindingMode.OneWay
        };

        Binding showHandlesBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.ShowHandles", Mode = BindingMode.TwoWay
        };

        Binding isSizeBoxEnabledBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.IsSizeBoxEnabled", Mode = BindingMode.TwoWay
        };

        Binding zoomboxAngleBinding = new() { Source = Viewport, Path = "Zoombox.Angle", Mode = BindingMode.OneWay };

        Binding scaleFromCenterBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.ScaleFromCenter", Mode = BindingMode.OneWay
        };

        Binding canAlignToPixelsBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.CanAlignToPixels", Mode = BindingMode.OneWay
        };

        Binding lockShearBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.LockShear", Mode = BindingMode.OneWay
        };

        Binding transformDraggedBinding = new()
        {
            Source = Viewport, Path = "Document.TransformViewModel.TransformDraggedCommand", Mode = BindingMode.OneWay
        };

        transformOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        transformOverlay.Bind(TransformOverlay.ActionCompletedProperty, actionCompletedBinding);
        transformOverlay.Bind(TransformOverlay.SnappingControllerProperty, snappingBinding);
        transformOverlay.Bind(TransformOverlay.CornersProperty, cornersBinding);
        transformOverlay.Bind(TransformOverlay.RequestCornersExecutorProperty, requestedCornersBinding);
        transformOverlay.Bind(TransformOverlay.CornerFreedomProperty, cornerFreedomBinding);
        transformOverlay.Bind(TransformOverlay.SideFreedomProperty, sideFreedomBinding);
        transformOverlay.Bind(TransformOverlay.LockRotationProperty, lockRotationBinding);
        transformOverlay.Bind(TransformOverlay.CoverWholeScreenProperty, coverWholeScreenBinding);
        transformOverlay.Bind(TransformOverlay.SnapToAnglesProperty, snapToAnglesBinding);
        transformOverlay.Bind(TransformOverlay.InternalStateProperty, internalStateBinding);
        transformOverlay.Bind(TransformOverlay.PassthroughPointerPressedCommandProperty, passThroughPointerPressedBinding);
        transformOverlay.Bind(TransformOverlay.ZoomboxAngleProperty, zoomboxAngleBinding);
        transformOverlay.Bind(TransformOverlay.ShowHandlesProperty, showHandlesBinding);
        transformOverlay.Bind(TransformOverlay.IsSizeBoxEnabledProperty, isSizeBoxEnabledBinding);
        transformOverlay.Bind(TransformOverlay.ScaleFromCenterProperty, scaleFromCenterBinding);
        transformOverlay.Bind(TransformOverlay.CanAlignToPixelsProperty, canAlignToPixelsBinding);
        transformOverlay.Bind(TransformOverlay.LockShearProperty, lockShearBinding);
        transformOverlay.Bind(TransformOverlay.TransformDraggedCommandProperty, transformDraggedBinding);
    }

    private void BindVectorPathOverlay()
    {
        Binding pathBinding = new()
        {
            Source = Viewport, Path = "Document.PathOverlayViewModel.Path", Mode = BindingMode.TwoWay
        };

        Binding addToUndoCommandBinding = new()
        {
            Source = Viewport, Path = "Document.PathOverlayViewModel.AddToUndoCommand", Mode = BindingMode.OneWay
        };

        Binding snappingBinding = new()
        {
            Source = Viewport, Path = "Document.SnappingViewModel.SnappingController", Mode = BindingMode.OneWay
        };

        vectorPathOverlay.Bind(VectorPathOverlay.PathProperty, pathBinding);
        vectorPathOverlay.Bind(VectorPathOverlay.AddToUndoCommandProperty, addToUndoCommandBinding);
        vectorPathOverlay.Bind(VectorPathOverlay.SnappingControllerProperty, snappingBinding);
    }

    private void BindSnappingOverlay()
    {
        Binding snappingControllerBinding = new()
        {
            Source = Viewport, Path = "Document.SnappingViewModel.SnappingController", Mode = BindingMode.OneWay
        };

        snappingOverlay.Bind(SnappingOverlay.SnappingControllerProperty, snappingControllerBinding);
    }

    private void BindMouseOverlayPointer()
    {
        Binding isTransformingBinding = new()
        {
            Source = Viewport, Path = "!Document.TransformViewModel.TransformActive", Mode = BindingMode.OneWay
        };

        Binding isOverCanvasBinding = new()
        {
            Source = Viewport, Path = "IsOverCanvas", Mode = BindingMode.OneWay
        };

        Binding brushSizeBinding = new()
        {
            Source = ViewModelMain.Current.ToolsSubViewModel, Path = "ActiveBasicToolbar.ToolSize", Mode = BindingMode.OneWay
        };

        Binding brushShapeBinding = new()
        {
            Source = ViewModelMain.Current.ToolsSubViewModel, Path = "ActiveTool.FinalBrushShape", Mode = BindingMode.OneWay
        };

        MultiBinding isVisibleMultiBinding = new()
        {
            Converter = new AllTrueConverter(),
            Mode = BindingMode.OneWay,
            Bindings = new List<IBinding>()
            {
                isTransformingBinding,
                isOverCanvasBinding
            }
        };

        brushShapeOverlay.Bind(Visual.IsVisibleProperty, isVisibleMultiBinding);
        brushShapeOverlay.Bind(BrushShapeOverlay.BrushSizeProperty, brushSizeBinding);
        brushShapeOverlay.Bind(BrushShapeOverlay.BrushShapeProperty, brushShapeBinding);
    }

    private void BindTextOverlay()
    {
        Binding isVisibleBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.IsActive", Mode = BindingMode.OneWay
        };

        Binding textBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.Text", Mode = BindingMode.TwoWay
        };

        Binding positionBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.Position", Mode = BindingMode.OneWay
        };

        Binding fontBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.Font", Mode = BindingMode.OneWay
        };

        Binding requestEditTextBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.RequestEditTextTrigger", Mode = BindingMode.OneWay
        };

        Binding matrixBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.Matrix", Mode = BindingMode.OneWay
        };

        Binding spacingBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.Spacing", Mode = BindingMode.OneWay
        };

        Binding cursorPositionBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.CursorPosition", Mode = BindingMode.TwoWay
        };

        Binding selectionEndBinding = new()
        {
            Source = Viewport, Path = "Document.TextOverlayViewModel.SelectionEnd", Mode = BindingMode.TwoWay
        };

        textOverlay.Bind(Visual.IsVisibleProperty, isVisibleBinding);
        textOverlay.Bind(TextOverlay.TextProperty, textBinding);
        textOverlay.Bind(TextOverlay.PositionProperty, positionBinding);
        textOverlay.Bind(TextOverlay.FontProperty, fontBinding);
        textOverlay.Bind(TextOverlay.RequestEditTextProperty, requestEditTextBinding);
        textOverlay.Bind(TextOverlay.MatrixProperty, matrixBinding);
        textOverlay.Bind(TextOverlay.SpacingProperty, spacingBinding);
        textOverlay.Bind(TextOverlay.CursorPositionProperty, cursorPositionBinding);
        textOverlay.Bind(TextOverlay.SelectionEndProperty, selectionEndBinding);
    }
}
