using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.Views.Overlays.Drawables;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Ruler;

public class RulerOverlay : Overlay
{
    public static readonly StyledProperty<MeasurementUnit> UnitProperty =
        AvaloniaProperty.Register<RulerOverlay, MeasurementUnit>(
            nameof(Unit), MeasurementUnit.Pixels);

    public static readonly StyledProperty<float> DpiProperty = AvaloniaProperty.Register<RulerOverlay, float>(
        nameof(Dpi));

    public static readonly StyledProperty<ShapeCorners> SelectionProperty =
        AvaloniaProperty.Register<RulerOverlay, ShapeCorners>(
            nameof(Selection));

    public static readonly StyledProperty<bool> SelectionEnabledProperty =
        AvaloniaProperty.Register<RulerOverlay, bool>(
            nameof(SelectionEnabled));

    public static readonly StyledProperty<ObservableCollection<IGuideHandler>> GuidesProperty =
        AvaloniaProperty.Register<RulerOverlay, ObservableCollection<IGuideHandler>>(
            nameof(Guides));

    public static readonly StyledProperty<ICommand> CreateGuideCommandProperty =
        AvaloniaProperty.Register<RulerOverlay, ICommand>(
            nameof(CreateGuideCommand));

    public static readonly StyledProperty<SnappingController> SnappingControllerProperty =
        AvaloniaProperty.Register<RulerOverlay, SnappingController>(
            nameof(SnappingController));

    public static readonly StyledProperty<VecI> DocumentSizeProperty = AvaloniaProperty.Register<RulerOverlay, VecI>(
        nameof(DocumentSize));

    public VecI DocumentSize
    {
        get => GetValue(DocumentSizeProperty);
        set => SetValue(DocumentSizeProperty, value);
    }

    public SnappingController SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    public ICommand CreateGuideCommand
    {
        get => GetValue(CreateGuideCommandProperty);
        set => SetValue(CreateGuideCommandProperty, value);
    }

    public ObservableCollection<IGuideHandler> Guides
    {
        get => GetValue(GuidesProperty);
        set => SetValue(GuidesProperty, value);
    }

    public bool SelectionEnabled
    {
        get => GetValue(SelectionEnabledProperty);
        set => SetValue(SelectionEnabledProperty, value);
    }

    public ShapeCorners Selection
    {
        get => GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
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

    public const double Offset = 24;

    private Paint backgroundPaint;
    private Paint textPaint;
    private Paint selectionTextPaint;
    private Paint highlightPaint;
    private Paint selectionPaint;

    private static Font textFont;

    private string unitText;
    private VecD lastPointerPos;
    private VecD lastCanvasPointer;

    private double previewGuide;
    private bool previewGuideIsVertical;
    private bool isDraggingPreviewGuide;

    private InfoBox infoBox;
    private Matrix3X3 lastMatrix;

    private const int SafeOverlapPadding = 8;

    private List<MeasurementUnit> allUnits =
    [
        MeasurementUnit.Pixels,
        MeasurementUnit.Millimeter,
        MeasurementUnit.Points,
        MeasurementUnit.Inches,
    ];

    static RulerOverlay()
    {
        textFont = Font.CreateDefault();
        UnitProperty.Changed.AddClassHandler<RulerOverlay, MeasurementUnit>(OnUnitChanged);
        GuidesProperty.Changed.AddClassHandler<RulerOverlay, ObservableCollection<IGuideHandler>>(OnGuidesChanged);
    }

    public RulerOverlay()
    {
        backgroundPaint = ResourceLoader.GetPaint("ThemeBackgroundBrush1");
        textPaint = ResourceLoader.GetPaint("ThemeForegroundLowBrush");
        highlightPaint = ResourceLoader.GetPaint("ThemeAccentHighBrush");
        selectionPaint = ResourceLoader.GetPaint("SelectionFillBrush");
        selectionTextPaint = ResourceLoader.GetPaint("ThemeAccent3Brush");
        textPaint.IsAntiAliased = true;
        selectionTextPaint.IsAntiAliased = true;
        unitText = new LocalizedString(MeasurementUnit.Pixels.ShortNameLocalizationKey);
        lastPointerPos = VecD.Zero;
        AlwaysPassPointerEvents = true;
        infoBox = new InfoBox();
    }


    private static void OnGuidesChanged(
        RulerOverlay overlay,
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

    private static void OnUnitChanged(RulerOverlay overlay,
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

    public override bool TestHit(VecD canvasPoint, VecD viewportPoint)
    {
        return viewportPoint.X < Offset || viewportPoint.Y < Offset;
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        int saved = context.Save();
        Matrix3X3 previousMatrix = context.TotalMatrix;
        lastMatrix = previousMatrix;

        context.SetMatrix(Matrix3X3.Identity);
        DrawHorizontalRuler(context, textPaint, previousMatrix);
        DrawVerticalRuler(context, textPaint, previousMatrix);

        context.DrawLine(new VecD(0, Offset), new VecD(context.DeviceClipBounds.Width, Offset), textPaint);
        context.DrawLine(new VecD(Offset, 0), new VecD(Offset, context.DeviceClipBounds.Height), textPaint);

        DrawUnit(context);
        DrawHighlight(context);

        context.RestoreToCount(saved);

        DrawGuides(context);
        DrawPreviewGuide(context);
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (TestHit(args.CanvasPoint, args.ViewportPoint))
        {
            if (args.PointerButton == MouseButton.Left)
            {
                if (args.ViewportPoint is { X: < Offset, Y: < Offset })
                {
                    int currentIndex = allUnits.IndexOf(Unit);
                    if (currentIndex == -1 || currentIndex == allUnits.Count - 1)
                    {
                        Unit = MeasurementUnit.Pixels;
                    }
                    else
                    {
                        Unit = allUnits[currentIndex + 1];
                    }
                }
                else if (args.ViewportPoint is { X: < Offset } or { Y: < Offset })
                {
                    previewGuideIsVertical = args.ViewportPoint.Y < Offset;
                    args.Pointer.Capture(this);
                    isDraggingPreviewGuide = true;

                    args.Handled = true;
                }

                Refresh();
                args.Handled = true;
            }
        }
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (isDraggingPreviewGuide)
        {
            previewGuide = previewGuideIsVertical ? args.CanvasPoint.X : args.CanvasPoint.Y;

            if (args.Modifiers.HasFlag(KeyModifiers.Shift))
            {
                double spacing;
                if (previewGuideIsVertical)
                {
                    GetYTickSpacing(lastMatrix, out spacing);
                }
                else
                {
                    GetXTickSpacing(lastMatrix, out spacing);
                }

                previewGuide = Unit.ToPixels(SnapToGrid(Unit.FromPixels(previewGuide, Dpi), spacing), Dpi);
            }

            Cursor = new Cursor(previewGuideIsVertical
                ? StandardCursorType.SizeWestEast
                : StandardCursorType.SizeNorthSouth);
        }

        lastPointerPos = args.ViewportPoint;
        lastCanvasPointer = args.CanvasPoint;
        Refresh();
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        if (isDraggingPreviewGuide)
        {
            isDraggingPreviewGuide = false;
            Cursor = null;
            CreateGuideCommand?.Execute((previewGuideIsVertical, previewGuide));
            Refresh();
        }
    }

    private static double SnapToGrid(double value, double spacing)
    {
        return Math.Round(value / spacing) * spacing;
    }

    private void DrawHighlight(Canvas context)
    {
        int saved = context.Save();
        context.ClipRect(new RectD(Offset, 0, context.DeviceClipBounds.Width, Offset));
        context.DrawLine(new VecD(lastPointerPos.X, 0), new VecD(lastPointerPos.X, Offset), highlightPaint);
        context.RestoreToCount(saved);

        saved = context.Save();
        context.ClipRect(new RectD(0, Offset, Offset, context.DeviceClipBounds.Height));
        context.DrawLine(new VecD(0, lastPointerPos.Y), new VecD(Offset, lastPointerPos.Y), highlightPaint);
        context.RestoreToCount(saved);
    }

    private void DrawGuides(Canvas context)
    {
        if (Guides == null)
            return;

        foreach (var guide in Guides)
        {
            if (guide.IsVertical)
            {
                context.DrawLine(new VecD(guide.OffsetBindable, 0), new VecD(guide.OffsetBindable, DocumentSize.Y),
                    highlightPaint);
            }
            else
            {
                context.DrawLine(new VecD(0, guide.OffsetBindable), new VecD(DocumentSize.X, guide.OffsetBindable),
                    highlightPaint);
            }
        }
    }

    private void DrawPreviewGuide(Canvas context)
    {
        if (isDraggingPreviewGuide)
        {
            if (previewGuideIsVertical)
            {
                context.DrawLine(new VecD(previewGuide, 0), new VecD(previewGuide, DocumentSize.Y),
                    highlightPaint);
            }
            else
            {
                context.DrawLine(new VecD(0, previewGuide), new VecD(DocumentSize.X, previewGuide),
                    highlightPaint);
            }

            infoBox.ZoomScale = ZoomScale;
            infoBox.DrawInfo(context,
                (Unit.FromPixels(previewGuide, Dpi)).ToString("0.####", CultureInfo.CurrentCulture) + $" {unitText}",
                lastCanvasPointer);
        }
    }

    private void DrawUnit(Canvas context)
    {
        var center = new VecD(Offset / 2f, Offset / 2f);
        textFont.MeasureText(unitText, out var measuredBounds, textPaint);
        VecD drawOn = center - measuredBounds.Center;
        context.DrawText(unitText, drawOn, TextAlign.Left, textFont, textPaint);
    }

    private (RectD, RectD)? DrawHorizontalSelection(Canvas context, Matrix3X3 matrix)
    {
        if (SelectionEnabled && Selection is { IsRect: true, RectSize: { X: > 0, Y: > 0 } })
        {
            RectD transformedSelection = matrix.TransformRect(Selection.AABBBounds);
            RectD selection = new RectD(transformedSelection.X, 0, transformedSelection.Width, Offset);
            context.DrawRect(selection, selectionPaint);

            var startValue = Unit.FromPixels(Selection.AABBBounds.X, Dpi);
            var endValue = Unit.FromPixels(Selection.AABBBounds.Right, Dpi);

            textFont.MeasureText(startValue.ToString("0.####"), out var measuredStartBounds, textPaint);
            textFont.MeasureText(endValue.ToString("0.####"), out var measuredEndBounds, textPaint);

            double yStart = selection.Center.Y - measuredStartBounds.Center.Y;
            double yEnd = selection.Center.Y - measuredEndBounds.Center.Y;

            context.DrawText(startValue.ToString("0.####"), new VecD(selection.Left - SafeOverlapPadding / 2f, yStart),
                TextAlign.Right, textFont, selectionTextPaint);
            context.DrawText(endValue.ToString("0.####"), new VecD(selection.Right + SafeOverlapPadding / 2f, yEnd),
                TextAlign.Left, textFont, selectionTextPaint);

            return (
                new RectD(selection.X - measuredStartBounds.Width - SafeOverlapPadding, selection.Y,
                    measuredStartBounds.Width + SafeOverlapPadding, selection.Height),
                new RectD(selection.Right - SafeOverlapPadding, selection.Y,
                    measuredEndBounds.Width + SafeOverlapPadding,
                    selection.Height));
        }

        return null;
    }

    private (RectD, RectD)? DrawVerticalSelection(Canvas context, Matrix3X3 matrix)
    {
        if (SelectionEnabled && Selection is { IsRect: true, RectSize: { X: > 0, Y: > 0 } })
        {
            RectD transformedSelection = matrix.TransformRect(Selection.AABBBounds);
            RectD selection = new RectD(0, transformedSelection.Y, Offset, transformedSelection.Height);
            context.DrawRect(selection, selectionPaint);

            var startValue = Unit.FromPixels(Selection.AABBBounds.Y, Dpi);
            var endValue = Unit.FromPixels(Selection.AABBBounds.Bottom, Dpi);

            string startText = startValue.ToString("0.####");
            string endText = endValue.ToString("0.####");

            textFont.MeasureText(startText, out var measuredStartBounds, textPaint);
            textFont.MeasureText(endText, out var measuredEndBounds, textPaint);

            double xStart = selection.Center.X - measuredStartBounds.Center.Y;
            double xEnd = selection.Center.X - measuredEndBounds.Center.Y;

            var startCenter = new VecD(xStart, selection.Top - SafeOverlapPadding / 2f);
            var endCenter = new VecD(xEnd, selection.Bottom + SafeOverlapPadding / 2f);

            context.Save();
            context.RotateDegrees(-90, (float)startCenter.X, (float)startCenter.Y);
            context.DrawText(startText, startCenter, TextAlign.Left, textFont, selectionTextPaint);
            context.Restore();

            context.Save();
            context.RotateDegrees(-90, (float)endCenter.X, (float)endCenter.Y);
            context.DrawText(endText, endCenter, TextAlign.Right, textFont, selectionTextPaint);
            context.Restore();

            return (
                new RectD(selection.X, selection.Top - measuredStartBounds.Width - SafeOverlapPadding, selection.Width,
                    measuredStartBounds.Width + SafeOverlapPadding),
                new RectD(selection.X, selection.Bottom - SafeOverlapPadding, selection.Width,
                    measuredEndBounds.Width + SafeOverlapPadding)
            );
        }

        return null;
    }

    private void DrawHorizontalRuler(Canvas context, Paint paint, Matrix3X3 previousMatrix)
    {
        var docSizePixelSpacing = GetXTickSpacing(previousMatrix, out var tickSpacing);

        RectD rect = new RectD(Offset, 0, context.DeviceClipBounds.Width, Offset);
        int saved = context.Save();
        context.DrawRect(rect with { Left = 0 }, backgroundPaint);
        context.ClipRect(rect);


        double startX = previousMatrix.TransX;
        double visibleStart = Offset;
        double visibleEnd = context.DeviceClipBounds.Width;

        double firstTick = Math.Ceiling((visibleStart - startX) / docSizePixelSpacing / tickSpacing) * tickSpacing;
        double lastTick = (visibleEnd - startX) / docSizePixelSpacing;

        var selectionRect = DrawHorizontalSelection(context, previousMatrix);

        for (double i = firstTick; i <= lastTick; i += tickSpacing)
        {
            double currentX = i * docSizePixelSpacing + startX;

            var targetPaint = paint;

            var measured = textFont.MeasureText(i.ToString("0.####"), out var measuredBounds, targetPaint);
            RectD rectOnRuler = new RectD(currentX - measured / 2f, 0, measured, Offset);
            if (selectionRect.HasValue && (selectionRect.Value.Item1.IntersectsWithInclusive(rectOnRuler) ||
                                           selectionRect.Value.Item2.IntersectsWithInclusive(rectOnRuler)))
            {
                continue;
            }

            context.DrawText(i.ToString("0.####"), new VecD(currentX, textFont.Size), TextAlign.Center, textFont,
                targetPaint);

            context.DrawLine(new VecD(currentX, Offset), new VecD(currentX, Offset - textFont.Size + 2), paint);
        }

        context.RestoreToCount(saved);
    }

    private double GetXTickSpacing(Matrix3X3 previousMatrix, out double tickSpacing)
    {
        double zeroX = previousMatrix.MapPoint(new VecD(0, 0)).X;
        double docSizePixelSpacing = previousMatrix.MapPoint(new VecD(Unit.ToPixels(1, Dpi), 0)).X - zeroX;
        tickSpacing = GetTickSpacing(GetDensityForUnit(), docSizePixelSpacing);
        return docSizePixelSpacing;
    }

    private void DrawVerticalRuler(Canvas context, Paint paint, Matrix3X3 previousMatrix)
    {
        var docSizePixelSpacing = GetYTickSpacing(previousMatrix, out var tickSpacing);

        RectD rect = new RectD(0, Offset, Offset, context.DeviceClipBounds.Height);
        int saved = context.Save();

        context.DrawRect(rect, backgroundPaint);

        var selectionRect = DrawVerticalSelection(context, previousMatrix);


        double startY = previousMatrix.TransY;
        double visibleStart = Offset;
        double visibleEnd = context.DeviceClipBounds.Height;

        double firstTick = Math.Ceiling((visibleStart - startY) / docSizePixelSpacing / tickSpacing) * tickSpacing;
        double lastTick = (visibleEnd - startY) / docSizePixelSpacing;

        for (double i = firstTick; i <= lastTick; i += tickSpacing)
        {
            double currentY = i * docSizePixelSpacing + startY;

            int save2 = context.Save();

            var textCenter = new VecD(textFont.Size, currentY);
            context.ClipRect(rect);
            context.RotateDegrees(-90, (float)textCenter.X, (float)textCenter.Y);

            textFont.MeasureText(i.ToString("0.####"), out var measuredBounds, paint);

            RectD rectOnRuler = new RectD(0, currentY - measuredBounds.Width / 2, Offset, measuredBounds.Width);

            context.RestoreToCount(save2);

            if (selectionRect.HasValue && (selectionRect.Value.Item1.IntersectsWithInclusive(rectOnRuler) ||
                                           selectionRect.Value.Item2.IntersectsWithInclusive(rectOnRuler)))
                continue;

            save2 = context.Save();
            context.ClipRect(rect);
            context.RotateDegrees(-90, (float)textCenter.X, (float)textCenter.Y);
            context.DrawText(i.ToString("0.####"), textCenter, TextAlign.Center, textFont, paint);
            context.RestoreToCount(save2);

            context.DrawLine(new VecD(Offset, currentY), new VecD(Offset - textFont.Size + 2, currentY), paint);
        }

        context.RestoreToCount(saved);
    }

    private double GetYTickSpacing(Matrix3X3 previousMatrix, out double tickSpacing)
    {
        double zeroY = previousMatrix.MapPoint(new VecD(0, 0)).Y;
        double docSizePixelSpacing = previousMatrix.MapPoint(new VecD(0, Unit.ToPixels(1, Dpi))).Y - zeroY;
        tickSpacing = GetTickSpacing(GetDensityForUnit(), docSizePixelSpacing);
        return docSizePixelSpacing;
    }

    private double GetDensityForUnit()
    {
        return 50;
    }

    private double GetTickSpacing(double minLabelSpacing, double docSizePixelSpacing)
    {
        double minTickSpacing = minLabelSpacing / docSizePixelSpacing;
        double tickSpacing = Unit.GetSpacing(minTickSpacing);
        if (!Unit.AllowFractionalValues)
        {
            tickSpacing = Math.Max(1, tickSpacing);
        }

        return tickSpacing;
    }
}
