using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls
{

    [ContentProperty("Text")]
    public class OutlinedTextBlock : FrameworkElement
    {
        private void UpdatePen()
        {
            _Pen = new Pen(Stroke, StrokeThickness)
            {
                DashCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round
            };

            InvalidateVisual();
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
          "Fill",
          typeof(Brush),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
          "Stroke",
          typeof(Brush),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback));

        private static void StrokePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            (dependencyObject as OutlinedTextBlock)?.UpdatePen();
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
          "StrokeThickness",
          typeof(double),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, StrokePropertyChangedCallback));

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
          "Text",
          typeof(string),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextInvalidated));

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
          "TextAlignment",
          typeof(TextAlignment),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
          "TextDecorations",
          typeof(TextDecorationCollection),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
          "TextTrimming",
          typeof(TextTrimming),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(OnFormattedTextUpdated));

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
          "TextWrapping",
          typeof(TextWrapping),
          typeof(OutlinedTextBlock),
          new FrameworkPropertyMetadata(TextWrapping.NoWrap, OnFormattedTextUpdated));

        private FormattedText _FormattedText;
        private Geometry _TextGeometry;
        private Pen _Pen;

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        public OutlinedTextBlock()
        {
            UpdatePen();
            TextDecorations = new TextDecorationCollection();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            EnsureGeometry();

            drawingContext.DrawGeometry(null, _Pen, _TextGeometry);
            drawingContext.DrawGeometry(Fill, null, _TextGeometry);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureFormattedText();

            // constrain the formatted text according to the available size

            double w = availableSize.Width;
            double h = availableSize.Height;

            // the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
            // the Math.Max call is to ensure we don't hit zero, which will cause MaxTextHeight to throw
            _FormattedText.MaxTextWidth = Math.Min(3579139, w);
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, h);

            // return the desired size
            return new Size(Math.Ceiling(_FormattedText.Width), Math.Ceiling(_FormattedText.Height));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureFormattedText();

            // update the formatted text with the final size
            _FormattedText.MaxTextWidth = finalSize.Width;
            _FormattedText.MaxTextHeight = Math.Max(0.0001d, finalSize.Height);

            // need to re-generate the geometry now that the dimensions have changed
            _TextGeometry = null;

            return finalSize;
        }

        private static void OnFormattedTextInvalidated(DependencyObject dependencyObject,
          DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock._FormattedText = null;
            outlinedTextBlock._TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private static void OnFormattedTextUpdated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock._TextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private void EnsureFormattedText()
        {
            if (_FormattedText != null)
            {
                return;
            }

            _FormattedText = new FormattedText(
              Text ?? "",
              CultureInfo.CurrentUICulture,
              FlowDirection,
              new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
              FontSize,
              Brushes.Black);

            UpdateFormattedText();
        }

        private void UpdateFormattedText()
        {
            if (_FormattedText == null)
            {
                return;
            }

            _FormattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            _FormattedText.TextAlignment = TextAlignment;
            _FormattedText.Trimming = TextTrimming;

            _FormattedText.SetFontSize(FontSize);
            _FormattedText.SetFontStyle(FontStyle);
            _FormattedText.SetFontWeight(FontWeight);
            _FormattedText.SetFontFamily(FontFamily);
            _FormattedText.SetFontStretch(FontStretch);
            _FormattedText.SetTextDecorations(TextDecorations);
        }

        private void EnsureGeometry()
        {
            if (_TextGeometry != null)
            {
                return;
            }

            EnsureFormattedText();
            _TextGeometry = _FormattedText.BuildGeometry(new Point(0, 0));
        }
    }
}