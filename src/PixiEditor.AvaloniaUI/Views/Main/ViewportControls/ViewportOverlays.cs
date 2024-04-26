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
using PixiEditor.AvaloniaUI.Views.Visuals;

namespace PixiEditor.AvaloniaUI.Views.Main.ViewportControls;

internal class ViewportOverlays
{
    public Viewport Viewport { get; set; }

    private GridLines gridLinesOverlay;
    private SelectionOverlay selectionOverlay;
    private SymmetryOverlay symmetryOverlay;
    private LineToolOverlay lineToolOverlay;

    public void Init(Viewport viewport)
    {
        Viewport = viewport;
        gridLinesOverlay = new GridLines();
        BindGridLines();

        selectionOverlay = new SelectionOverlay();
        BindSelectionOverlay();

        symmetryOverlay = new SymmetryOverlay();
        BindSymmetryOverlay();

        lineToolOverlay = new LineToolOverlay();
        BindLineToolOverlay();

        Viewport.ActiveOverlays.Add(gridLinesOverlay);
        Viewport.ActiveOverlays.Add(selectionOverlay);
        Viewport.ActiveOverlays.Add(symmetryOverlay);
        Viewport.ActiveOverlays.Add(lineToolOverlay);
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

        gridLinesOverlay.Bind(GridLines.PixelWidthProperty, binding);
        gridLinesOverlay.Bind(GridLines.ColumnsProperty, binding);

        binding = new Binding
        {
            Source = Viewport,
            Path = "Document.Height",
            Mode = BindingMode.OneWay
        };

        gridLinesOverlay.Bind(GridLines.PixelHeightProperty, binding);
        gridLinesOverlay.Bind(GridLines.RowsProperty, binding);
        gridLinesOverlay.Bind(Visual.IsVisibleProperty, isVisBinding);
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
}
