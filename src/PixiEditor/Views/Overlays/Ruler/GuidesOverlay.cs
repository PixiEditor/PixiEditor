using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Overlays.Drawables;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Ruler;

public class GuidesOverlay : Overlay
{
    public static readonly StyledProperty<ObservableCollection<IGuideHandler>> GuidesProperty =
        AvaloniaProperty.Register<RulerOverlay, ObservableCollection<IGuideHandler>>(
            nameof(Guides));

    public static readonly StyledProperty<VecI> DocumentSizeProperty = AvaloniaProperty.Register<RulerOverlay, VecI>(
        nameof(DocumentSize));

    public static readonly StyledProperty<ICommand> BeginDragGuideCommandProperty =
        AvaloniaProperty.Register<GuidesOverlay, ICommand>(
            nameof(BeginDragGuideCommand));

    public static readonly StyledProperty<ICommand> DragGuideCommandProperty =
        AvaloniaProperty.Register<GuidesOverlay, ICommand>(
            nameof(DragGuideCommand));

    public static readonly StyledProperty<ICommand> EndDragGuideCommandProperty =
        AvaloniaProperty.Register<GuidesOverlay, ICommand>(
            nameof(EndDragGuideCommand));

    public static readonly StyledProperty<MeasurementUnit> UnitProperty =
        AvaloniaProperty.Register<RulerOverlay, MeasurementUnit>(
            nameof(Unit), MeasurementUnit.Pixels);

    public static readonly StyledProperty<float> DpiProperty = AvaloniaProperty.Register<RulerOverlay, float>(
        nameof(Dpi));

    public ICommand EndDragGuideCommand
    {
        get => GetValue(EndDragGuideCommandProperty);
        set => SetValue(EndDragGuideCommandProperty, value);
    }

    public ICommand DragGuideCommand
    {
        get => GetValue(DragGuideCommandProperty);
        set => SetValue(DragGuideCommandProperty, value);
    }

    public ICommand BeginDragGuideCommand
    {
        get => GetValue(BeginDragGuideCommandProperty);
        set => SetValue(BeginDragGuideCommandProperty, value);
    }

    public VecI DocumentSize
    {
        get => GetValue(DocumentSizeProperty);
        set => SetValue(DocumentSizeProperty, value);
    }

    public ObservableCollection<IGuideHandler> Guides
    {
        get => GetValue(GuidesProperty);
        set => SetValue(GuidesProperty, value);
    }

    public float Dpi
    {
        get => GetValue(DpiProperty);
        set => SetValue(DpiProperty, value);
    }

    public MeasurementUnit Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public static readonly StyledProperty<bool> CanEditProperty = AvaloniaProperty.Register<GuidesOverlay, bool>(
        nameof(CanEdit));

    public bool CanEdit
    {
        get => GetValue(CanEditProperty);
        set => SetValue(CanEditProperty, value);
    }

    private Paint guidePaint;
    private const double HitDistance = 0.5;

    private IGuideHandler? capturedGuide;
    private Matrix3X3 lastMatrix;
    private InfoBox infoBox;
    private string unitText;
    private VecD lastCanvasPointer;

    static GuidesOverlay()
    {
        GuidesProperty.Changed.AddClassHandler<GuidesOverlay, ObservableCollection<IGuideHandler>>(OnGuidesChanged);
        UnitProperty.Changed.AddClassHandler<GuidesOverlay, MeasurementUnit>(OnUnitChanged);
    }

    public GuidesOverlay()
    {
        guidePaint = ResourceLoader.GetPaint("ThemeAccent3Brush");
        guidePaint.BlendMode = BlendMode.HardLight;
        infoBox = new InfoBox();
        unitText = new LocalizedString(MeasurementUnit.Pixels.ShortNameLocalizationKey);
    }

    private static void OnUnitChanged(GuidesOverlay overlay,
        AvaloniaPropertyChangedEventArgs<MeasurementUnit> avaloniaPropertyChangedEventArgs)
    {
        if (avaloniaPropertyChangedEventArgs.NewValue.Value != null)
        {
            overlay.unitText =
                new LocalizedString(avaloniaPropertyChangedEventArgs.NewValue.Value.ShortNameLocalizationKey);
        }
        else
        {
            overlay.unitText = new LocalizedString("UNKNOWN_UNIT_SHORT_NAME");
        }
    }

    private static void OnGuidesChanged(GuidesOverlay overlay,
        AvaloniaPropertyChangedEventArgs<ObservableCollection<IGuideHandler>> args)
    {
        if (args.OldValue.Value is { } oldGuides)
            oldGuides.CollectionChanged -= overlay.OnGuidesCollectionChanged;

        if (args.NewValue.Value is { } newGuides)
            newGuides.CollectionChanged += overlay.OnGuidesCollectionChanged;

        overlay.Refresh();
    }

    private void OnGuidesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Refresh();
    }

    public override bool TestHit(VecD canvasPoint, VecD viewportPoint)
    {
        return CanEdit && Guides.Any(x =>
            (x.IsVertical && Math.Abs(canvasPoint.X - x.OffsetBindable) < HitDistance) ||
            (!x.IsVertical && Math.Abs(canvasPoint.Y - x.OffsetBindable) < HitDistance)
        );
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        DrawGuides(context);
        if (capturedGuide != null)
        {
            infoBox.ZoomScale = ZoomScale;
            infoBox.DrawInfo(context,
                (Unit.FromPixels(capturedGuide.OffsetBindable, Dpi)).ToString("0.####", CultureInfo.CurrentCulture) +
                $" {unitText}",
                lastCanvasPointer);
        }

        lastMatrix = context.TotalMatrix;
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left || !CanEdit) return;
        var canvasPoint = args.CanvasPoint;
        var guide = Guides.FirstOrDefault(x =>
            (x.IsVertical && Math.Abs(canvasPoint.X - x.OffsetBindable) < HitDistance) ||
            (!x.IsVertical && Math.Abs(canvasPoint.Y - x.OffsetBindable) < HitDistance));

        capturedGuide = guide;
        args.Pointer.Capture(this);
        BeginDragGuideCommand?.Execute(guide.Id);
        args.Handled = true;
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (capturedGuide == null) return;
        double offset = capturedGuide.IsVertical ? args.CanvasPoint.X : args.CanvasPoint.Y;

        if (args.Modifiers.HasFlag(KeyModifiers.Shift))
        {
            double spacing;
            if (capturedGuide.IsVertical)
            {
                RulerOverlay.GetYTickSpacing(lastMatrix, Unit, Dpi, out spacing);
            }
            else
            {
                RulerOverlay.GetXTickSpacing(lastMatrix, Unit, Dpi, out spacing);
            }

            offset = Unit.ToPixels(RulerOverlay.SnapToGrid(Unit.FromPixels(offset, Dpi), spacing), Dpi);
        }

        DragGuideCommand?.Execute(new GuideDragInfo(capturedGuide.Id, offset));
        Cursor = new Cursor(capturedGuide.IsVertical
            ? StandardCursorType.SizeWestEast
            : StandardCursorType.SizeNorthSouth);
        lastCanvasPointer = args.CanvasPoint;
    }

    protected override void OnOverlayPointerEntered(OverlayPointerArgs args)
    {
        var canvasPoint = args.CanvasPoint;
        var guide = Guides.FirstOrDefault(x =>
            (x.IsVertical && Math.Abs(canvasPoint.X - x.OffsetBindable) < HitDistance) ||
            (!x.IsVertical && Math.Abs(canvasPoint.Y - x.OffsetBindable) < HitDistance));

        Cursor = new Cursor(guide.IsVertical ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth);
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        if (capturedGuide == null) return;
        EndDragGuideCommand?.Execute(null);
        capturedGuide = null;
    }

    private void DrawGuides(Canvas context)
    {
        if (Guides == null)
            return;

        guidePaint.StrokeWidth = 1.5f / (float)ZoomScale;
        foreach (var guide in Guides)
        {
            if (guide.IsVertical)
            {
                context.DrawLine(new VecD(guide.OffsetBindable, 0), new VecD(guide.OffsetBindable, DocumentSize.Y),
                    guidePaint);
            }
            else
            {
                context.DrawLine(new VecD(0, guide.OffsetBindable), new VecD(DocumentSize.X, guide.OffsetBindable),
                    guidePaint);
            }
        }
    }
}
