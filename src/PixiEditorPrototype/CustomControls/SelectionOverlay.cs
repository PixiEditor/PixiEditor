using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using SkiaSharp;

namespace PixiEditorPrototype.CustomControls;

public class SelectionOverlay : Control
{
    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register(nameof(Path), typeof(SKPath), typeof(SelectionOverlay),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(SelectionOverlay), new(1.0, OnZoomboxScaleChanged));

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public SKPath? Path
    {
        get => (SKPath?)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    private Pen whitePen = new Pen(Brushes.White, 1);
    private Pen blackDashedPen = new Pen(Brushes.Black, 1) { DashStyle = frame7 };

    private static DashStyle frame1 = new DashStyle(new double[] { 2, 4 }, 0);
    private static DashStyle frame2 = new DashStyle(new double[] { 2, 4 }, 1);
    private static DashStyle frame3 = new DashStyle(new double[] { 2, 4 }, 2);
    private static DashStyle frame4 = new DashStyle(new double[] { 2, 4 }, 3);
    private static DashStyle frame5 = new DashStyle(new double[] { 2, 4 }, 4);
    private static DashStyle frame6 = new DashStyle(new double[] { 2, 4 }, 5);
    private static DashStyle frame7 = new DashStyle(new double[] { 2, 4 }, 6);

    private Geometry renderPath = new PathGeometry();
    private PathFigureCollectionConverter converter = new();

    public SelectionOverlay()
    {
        IsHitTestVisible = false;

        blackDashedPen.BeginAnimation(Pen.DashStyleProperty, new ObjectAnimationUsingKeyFrames()
        {
            KeyFrames = new ObjectKeyFrameCollection()
            {
                new DiscreteObjectKeyFrame(frame1, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame2, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame3, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame4, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame5, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame6, KeyTime.Paced),
                new DiscreteObjectKeyFrame(frame7, KeyTime.Paced),
            },
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500))
        });
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (Path is null)
            return;

        renderPath = new PathGeometry()
        {
            FillRule = FillRule.Nonzero,
            Figures = (PathFigureCollection?)converter.ConvertFromString(Path.ToSvgPathData()),
        };
        drawingContext.DrawGeometry(null, whitePen, renderPath);
        drawingContext.DrawGeometry(null, blackDashedPen, renderPath);
    }

    private static void OnZoomboxScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (SelectionOverlay)obj;
        double newScale = (double)args.NewValue;
        self.whitePen.Thickness = 1.0 / newScale;
        self.blackDashedPen.Thickness = 1.0 / newScale;
    }
}
