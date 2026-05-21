using Avalonia;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Helpers;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.ColorPickerOverlay;

internal class ColorPickerPreviewOverlay : Overlay
{
    public static readonly StyledProperty<Color> HoveredColorProperty =
        AvaloniaProperty.Register<ColorPickerPreviewOverlay, Color>(nameof(HoveredColor));

    public Color HoveredColor
    {
        get => GetValue(HoveredColorProperty);
        set => SetValue(HoveredColorProperty, value);
    }

    private const float OffsetX = 12;
    private const float OffsetY = -40;
    private const float Padding = 8;
    private const float ColorPreviewSize = 18;
    private const float ColorPreviewCornerRadius = 4;
    private const float PopupHeight = 30;
    private const float FontSize = 13;
    private const float CornerRadius = 5;
    private const float CheckerSquareSize = 4f;
    private static readonly Color CheckerLight = new Color(97, 97, 97);
    private static readonly Color CheckerDark = new Color(53, 53, 53);

    private readonly Paint backgroundPaint;
    private readonly Paint borderPaint;
    private readonly Paint previewPaint;
    private readonly Paint checkerPaint;
    private readonly Paint textPaint;
    private readonly Font font;

    public Func<VecD, Color>? ColorSampler { get; set; }

    public ColorPickerPreviewOverlay()
    {
        AlwaysPassPointerEvents = true;

        backgroundPaint = new Paint { Style = PaintStyle.Fill, IsAntiAliased = true };
        borderPaint = new Paint { Style = PaintStyle.Stroke, IsAntiAliased = true };
        previewPaint = new Paint { Style = PaintStyle.Fill, IsAntiAliased = true };
        checkerPaint = new Paint { Style = PaintStyle.Fill, IsAntiAliased = false };
        textPaint = new Paint { Style = PaintStyle.Fill, IsAntiAliased = true };

        font = ThemeResources.ThemeFont;
        backgroundPaint.Color = ThemeResources.BackgroundColor;
        borderPaint.Color = ThemeResources.BorderMidColor;
        textPaint.Color = ThemeResources.ForegroundColor;
    }

    private string _cachedHexText = "#000000";
    private float _cachedTextWidth = 0;
    private float _cachedPopupWidth = 0;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty && change.GetNewValue<bool>() && ColorSampler != null)
        {
            SampleAndCache(PointerPosition);
            Refresh();
        }
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (ColorSampler != null)
            SampleAndCache(args.Point);
        Refresh();
    }

    private void SampleAndCache(VecD point)
    {
        HoveredColor = ColorSampler!(point);

        _cachedHexText = HoveredColor.A == 255
            ? $"#{HoveredColor.R:X2}{HoveredColor.G:X2}{HoveredColor.B:X2}"
            : $"#{HoveredColor.R:X2}{HoveredColor.G:X2}{HoveredColor.B:X2}{HoveredColor.A:X2}";

        _cachedTextWidth = (float)font.MeasureText(_cachedHexText);
        _cachedPopupWidth = ColorPreviewSize + Padding * 3 + _cachedTextWidth;
    }

    public override bool TestHit(VecD point) => false;

    private void DrawCheckerboard(Canvas context, float x, float y)
    {
        int savedState = context.Save();
        context.ClipRoundRect(
            new RectD(x, y, ColorPreviewSize, ColorPreviewSize),
            new VecD(ColorPreviewCornerRadius, ColorPreviewCornerRadius),
            ClipOperation.Intersect);

        checkerPaint.Color = CheckerLight;
        context.DrawRect(x, y, ColorPreviewSize, ColorPreviewSize, checkerPaint);

        checkerPaint.Color = CheckerDark;
        int squares = (int)Math.Ceiling(ColorPreviewSize / CheckerSquareSize);
        for (int row = 0; row < squares; row++)
        {
            for (int col = 0; col < squares; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    context.DrawRect(
                        x + col * CheckerSquareSize,
                        y + row * CheckerSquareSize,
                        CheckerSquareSize,
                        CheckerSquareSize,
                        checkerPaint);
                }
            }
        }

        context.RestoreToCount(savedState);
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds)
    {
        font.Size = FontSize;

        int savedState = context.Save();
        var matrix = context.TotalMatrix;
        VecD screenPos = matrix.MapPoint(PointerPosition);
        context.SetMatrix(Matrix3X3.Identity);

        float x = (float)screenPos.X + OffsetX;
        float y = (float)screenPos.Y + OffsetY;

        context.DrawRoundRect(x, y, _cachedPopupWidth, PopupHeight, CornerRadius, CornerRadius, backgroundPaint);

        borderPaint.StrokeWidth = 1;
        context.DrawRoundRect(x, y, _cachedPopupWidth, PopupHeight, CornerRadius, CornerRadius, borderPaint);

        float previewX = x + Padding;
        float previewY = y + (PopupHeight - ColorPreviewSize) / 2;

        if (HoveredColor.A < 255)
            DrawCheckerboard(context, previewX, previewY);

        previewPaint.Color = HoveredColor;
        context.DrawRoundRect(previewX, previewY, ColorPreviewSize, ColorPreviewSize, ColorPreviewCornerRadius, ColorPreviewCornerRadius, previewPaint);
        context.DrawRoundRect(previewX, previewY, ColorPreviewSize, ColorPreviewSize, ColorPreviewCornerRadius, ColorPreviewCornerRadius, borderPaint);

        float textX = previewX + ColorPreviewSize + Padding;
        float textY = y + PopupHeight / 2 + FontSize / 2 - 1;

        context.DrawText(_cachedHexText, new VecD(textX, textY), TextAlign.Left, font, textPaint);

        context.RestoreToCount(savedState);
    }
}