using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hardware.Info;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Guides;

namespace PixiEditor.Models.DataHolders.Guides
{
    internal class LineGuide : Guide
    {
        private VecD position;
        private double rotation;
        private Color color;

        public VecD Position
        {
            get => position;
            set
            {
                if (SetProperty(ref position, value))
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
            var m = Math.Tan(Rotation);

            var penThickness = renderer.ScreenUnit;

            var brush = new SolidColorBrush(Color);
            var pen = new Pen(brush, penThickness * 1.5d);
            var points = GetIntersectionsInside(documentSize);
            context.DrawLine(pen, points.Item1, points.Item2);

            if (ShowExtended || ShowEditable)
            {
                var scale = ShowEditable ? 6 : 3;
                context.DrawEllipse(Brushes.Aqua, null, new Point(Position.X, Position.Y), penThickness * scale, penThickness * scale);
            }
        }

        private Point[] GetIntersections(VecI size)
        {
            var points = new Point[4];

            var m = Math.Tan(Rotation);

            points[0] = new Point(0, m * Position.X + Position.Y);
            points[1] = new Point(Position.X + Position.Y / m, 0);
            points[2] = new Point(size.X, -m * size.X + m * Position.X + Position.Y);
            points[3] = new Point(Position.X + Position.Y / m - size.Y / m, size.Y);

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
