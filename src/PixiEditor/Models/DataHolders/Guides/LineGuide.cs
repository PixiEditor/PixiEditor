using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Guides;
using PixiEditor.Views.UserControls.Overlays.TransformOverlay;

namespace PixiEditor.Models.DataHolders.Guides
{
    internal class LineGuide : Guide
    {
        private VecD position;
        private bool? isMoving;
        private double rotation;
        private Color color;
        private double x;
        private double y;
        private Pen blackPen = new Pen(Brushes.Black, 1);
        private GuideRenderer focusedRenderer;

        private PathGeometry rotateCursorGeometry = new()
        {
            Figures = (PathFigureCollection?)new PathFigureCollectionConverter()
            .ConvertFrom("M -1.26 -0.455 Q 0 0.175 1.26 -0.455 L 1.12 -0.735 L 2.1 -0.7 L 1.54 0.105 L 1.4 -0.175 Q 0 0.525 -1.4 -0.175 L -1.54 0.105 L -2.1 -0.7 L -1.12 -0.735 Z"),
        };

        public double X
        {
            get => x;
            set
            {
                if (SetProperty(ref x, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public double Y
        {
            get => y;
            set
            {
                if (SetProperty(ref y, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public double Rotation
        {
            get => rotation;
            set
            {
                if (SetProperty(ref rotation, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                if (SetProperty(ref color, value))
                {
                    InvalidateVisual();
                }
            }
        }

        public override string TypeNameKey => "LINE_GUIDE";

        public override Control SettingsControl { get; }

        public LineGuide(DocumentViewModel document) : base(document)
        {
            Color = Colors.CadetBlue;
            SettingsControl = new LineGuideSettings(this);
        }

        public override void Draw(DrawingContext context, GuideRenderer renderer)
        {
            var documentSize = Document.SizeBindable;

            var penThickness = renderer.ScreenUnit;

            var brush = new SolidColorBrush(Color);
            var points = GetIntersectionsInside(documentSize);

            if (ShowExtended || IsEditing)
            {
                var scale = IsEditing ? 3 : 1.5;
                var pen = new Pen(brush, penThickness * 2 * scale);
                context.DrawLine(pen, points.Item1, points.Item2);
                context.DrawEllipse(Brushes.Aqua, null, new Point(X, Y), penThickness * 2 * scale, penThickness * 2 * scale);
            }
            else
            {
                var pen = new Pen(brush, penThickness * 1.5d);
                context.DrawLine(pen, points.Item1, points.Item2);
            }

            blackPen.Thickness = penThickness;
            if (renderer == focusedRenderer)
            {
                context.DrawGeometry(Brushes.White, blackPen, rotateCursorGeometry);
            }
        }

        private bool UpdateRotationCursor(VecD mousePos, bool show, GuideRenderer renderer)
        {
            if (!show)
            {
                rotateCursorGeometry.Transform = new ScaleTransform(0, 0);
                return false;
            }
            else
            {
                var matrix = new TranslateTransform(mousePos.X, mousePos.Y).Value;
                var vec = mousePos - new VecD(X, Y);
                matrix.RotateAt(vec.Angle * 180 / Math.PI - 90, mousePos.X, mousePos.Y);
                matrix.ScaleAt(8 / renderer.ZoomboxScale, 8 / renderer.ZoomboxScale, mousePos.X, mousePos.Y);
                vec = vec.Normalize();
                matrix.Translate(vec.X, vec.Y);
                rotateCursorGeometry.Transform = new MatrixTransform(matrix);
                return true;
            }
        }

        protected override void RendererAttached(GuideRenderer renderer)
        {
            renderer.MouseEnter += Renderer_MouseEnter;
            renderer.MouseLeave += Renderer_MouseLeave;

            renderer.MouseLeftButtonDown += Renderer_MouseLeftButtonDown;
            renderer.MouseMove += Renderer_MouseMove;
            renderer.MouseLeftButtonUp += Renderer_MouseLeftButtonUp;

            renderer.MouseWheel += Renderer_MouseWheel;
        }

        private void Renderer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            e.Handled = true;
            focusedRenderer = (GuideRenderer)sender;
            var mousePos = e.GetPosition(focusedRenderer);
            var shouldMove = ShouldMove(focusedRenderer, mousePos);
            UpdateRotationCursor(TransformHelper.ToVecD(mousePos), !shouldMove, focusedRenderer);

            if (shouldMove)
            {
                focusedRenderer.Cursor = Cursors.Cross;
            }
            else
            {
                focusedRenderer.Cursor = Cursors.None;
            }

            focusedRenderer.InvalidateVisual();
        }

        private void Renderer_MouseLeave(object sender, MouseEventArgs e)
        {
            focusedRenderer = (GuideRenderer)sender;
            focusedRenderer.Cursor = null;
            focusedRenderer.InvalidateVisual();
            focusedRenderer = null;
        }

        private void Renderer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            e.Handled = true;
            focusedRenderer = (GuideRenderer)sender;
            Mouse.Capture(focusedRenderer);

            var mousePos = e.GetPosition(focusedRenderer);
            isMoving = ShouldMove(focusedRenderer, mousePos);
        }

        private void Renderer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEditing)
            {
                return;
            }

            var focusedRenderer = (GuideRenderer)sender;
            var mousePos = e.GetPosition(focusedRenderer);
            var x = mousePos.X;
            var y = mousePos.Y;
            var vecD = new VecD(x, y);

            var shouldMove = ShouldMove(focusedRenderer, mousePos);

            if (shouldMove)
            {
                focusedRenderer.Cursor = Cursors.Cross;
            }
            else
            {
                focusedRenderer.Cursor = Cursors.None;
            }


            if (isMoving == null)
            {
                UpdateRotationCursor(vecD, !shouldMove, focusedRenderer);
                return;
            }

            focusedRenderer.InvalidateVisual();

            if (isMoving.Value)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    X = Math.Round(x, MidpointRounding.AwayFromZero);
                    Y = Math.Round(y, MidpointRounding.AwayFromZero);
                }
                else
                {
                    X = Math.Round(x * 2, MidpointRounding.AwayFromZero) / 2;
                    Y = Math.Round(y * 2, MidpointRounding.AwayFromZero) / 2;
                }
            }
            else
            {
                Rotation = Math.Round((vecD - new VecD(X, Y)).Angle * -1 * (180 / Math.PI), 1, MidpointRounding.AwayFromZero);
                UpdateRotationCursor(vecD, true, focusedRenderer);
            }
        }

        private void Renderer_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isMoving == null)
            {
                return;
            }

            Mouse.Capture(null);
            e.Handled = true;
            focusedRenderer = null;
            isMoving = null;
            var renderer = (GuideRenderer)sender;
            renderer.Cursor = null;
            renderer.InvalidateVisual();
        }

        private void Renderer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var renderer = (GuideRenderer)sender;

            if (ShouldMove(renderer, e.GetPosition(renderer)))
            {
                e.Handled = true;
                Rotation += e.Delta / (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.LeftShift) ? 50 : 10);
            }
        }

        private bool ShouldMove(GuideRenderer renderer, Point mousePos) => (mousePos - new Point(X, Y)).Length < 10 * renderer.ScreenUnit;

        private Point[] GetIntersections(VecI size)
        {
            var points = new Point[4];

            var m = Math.Tan(Rotation * (Math.PI / 180));

            points[0] = new Point(0, m * X + Y);
            points[1] = new Point(X + Y / m, 0);
            points[2] = new Point(size.X, -m * size.X + m * X + Y);
            points[3] = new Point(X + Y / m - size.Y / m, size.Y);

            return points;
        }

        private (Point, Point) GetIntersectionsInside(VecI size)
        {
            var points = GetIntersections(size).Where(x => PointInside(x, size)).ToArray();

            if (points.Length < 2)
            {
                throw new IndexOutOfRangeException("Guide did not have enough intersection points");
            }

            return (points[0], points[1]);
        }

        private bool PointInside(Point point, VecI size) => point.X >= 0 && point.X <= size.X && point.Y >= 0 && point.Y <= size.Y;
    }
}
