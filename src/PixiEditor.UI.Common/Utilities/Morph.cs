using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using PixiEditor.UI.Common.Extensions;

namespace PixiEditor.UI.Common.Utilities
{
    public static class Morph
    {
        public static bool Collapse(PathGeometry sourceGeometry, double progress)
        {
            int count = sourceGeometry.Figures.Count;
            for (int i = 0; i < sourceGeometry.Figures.Count; i++)
            {
                count -= MorphCollapse(sourceGeometry.Figures[i], progress);
            }

            if (count <= 0) return true;

            return false;
        }

        private static void MoveFigure(PathFigure source, double p, double progress)
        {
            var segment = (PolyLineSegment)source.Segments[0];

            for (int i = 0; i < segment.Points.Count; i++)
            {
                var fromX = segment.Points[i].X;
                var fromY = segment.Points[i].Y;

                var x = fromX + p;
                segment.Points[i] = new Point(x, fromY);
            }

            var newX = source.StartPoint.X + p;

            source.StartPoint = new Point(newX, source.StartPoint.Y);
        }
/* TODO:
        private static bool DoFiguresOverlap(PathFigures figures, int index0, int index1, int index2)
        {
            if (index2 < figures.Count && index0 >= 0)
            {
                var g0 = new PathGeometry();
                g0.Figures.Add(figures[index2]);
                var g1 = new PathGeometry();
                g1.Figures.Add(figures[index1]);
                var g2 = new PathGeometry();
                g2.Figures.Add(figures[index0]);
                // TODO: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.geometry.fillcontainswithdetail?view=net-5.0
                var result0 = g0.FillContainsWithDetail(g1);
                var result1 = g0.FillContainsWithDetail(g2);

                return
                    (result0 == IntersectionDetail.FullyContains ||
                        result0 == IntersectionDetail.FullyInside) &&
                    (result1 == IntersectionDetail.FullyContains ||
                        result1 == IntersectionDetail.FullyInside);
            }

            return false;
        }

        private static bool DoFiguresOverlap(PathFigures figures, int index0, int index1)
        {
            if (index1 < figures.Count && index0 >= 0)
            {
                var g1 = new PathGeometry();
                g1.Figures.Add(figures[index1]);
                var g2 = new PathGeometry();
                g2.Figures.Add(figures[index0]);
                // TODO: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.geometry.fillcontainswithdetail?view=net-5.0
                var result = g1.FillContainsWithDetail(g2);
                return result == IntersectionDetail.FullyContains || result == IntersectionDetail.FullyInside;
            }
            return false;
        }
*/
        private static void CollapseFigure(PathFigure figure)
        {
            var points = ((PolyLineSegment)figure.Segments[0]).Points;
            var centroid = GetCentroid(points);

            for (int p = 0; p < points.Count; p++)
            {
                points[p] = centroid;
            }

            figure.StartPoint = centroid;
        }

        public static void To(PathGeometry sourceGeometry, PathGeometry geometry, Range sourceRange, double progress)
        {
            int k = 0;
            for (int i = sourceRange.Start.Value; i < sourceRange.End.Value; i++)
            {
                MorphFigure(sourceGeometry.Figures[i], geometry.Figures[k], progress);
                k++;
            }
        }

        public static List<PathGeometry> ToCache(PathGeometry source, PathGeometry target, double speed, IEasing easing)
        {
            int steps = (int)(1 / speed);
            double p = speed;
            var cache = new List<PathGeometry>(steps);

            // TODO: wasn't present in original
            cache.Add(source.ClonePathGeometry());
            
            for (int i = 0; i < steps; i++)
            {
                var clone = source.ClonePathGeometry();
                var easeP = easing.Ease(p);

                To(clone, target, easeP);

                p += speed;

                cache.Add(clone);
            }

            // TODO: wasn't present in original
            cache.Add(target.ClonePathGeometry());

            return cache;
        }

        public static void To(PathGeometry source, PathGeometry target, double progress)
        {
            //
            // Clone figures.
            //
            if (source.Figures.Count < target.Figures.Count)
            {
                var last = source.Figures.Last();
                var toAdd = target.Figures.Count - source.Figures.Count;
                for (int i = 0; i < toAdd; i++)
                {
                    var clone = last.ClonePathFigure();
                    source.Figures.Add(clone);
                }
            }
            //
            // Contract the source, the problem here is that if we have a shape
            // like 'O' where we need to cut a hole in a shape we will butcher such character
            // since all excess shapes will be stored under this shape.
            //
            // We need to move and collapse them when moving.
            // So lets collapse then to a single point.
            //
            else if (source.Figures.Count > target.Figures.Count)
            {
                var toAdd = source.Figures.Count - target.Figures.Count;
                var lastIndex = target.Figures.Count - 1;

                for (int i = 0; i < toAdd; i++)
                {
                    var clone = target.Figures[lastIndex].ClonePathFigure();
                    //var clone = target.Figures[(lastIndex - (i % (lastIndex + 1)))].Clone();

                    //
                    // This is a temp solution but it works well for now.
                    // We try to detect if our last shape has an overlapping geometry
                    // if it does then we will clone the previrous shape.
                    //
                    if (lastIndex > 0)
                    {
                        /* TODO:
                        if (DoFiguresOverlap(target.Figures, lastIndex - 1, lastIndex))
                        {
                            if (DoFiguresOverlap(target.Figures, lastIndex - 2, lastIndex - 1, lastIndex))
                            {
                                clone = target.Figures[lastIndex - 3].ClonePathFigure();
                            }
                            else if (lastIndex - 2 > 0)
                            {
                                clone = target.Figures[lastIndex - 2].ClonePathFigure();
                            }
                            else
                            {
                                CollapseFigure(clone);
                            }
                        }
                        //*/
                    }
                    else
                    {
                        CollapseFigure(clone);
                    }

                    target.Figures.Add(clone);
                }
            }

            int[] map = new int[source.Figures.Count];
            for (int i = 0; i < map.Length; i++)
                map[i] = -1;

            //
            // Morph Closest Figures.
            //
            for (int i = 0; i < source.Figures.Count; i++)
            {
                double closest = double.MaxValue;
                int closestIndex = -1;

                for (int j = 0; j < target.Figures.Count; j++)
                {
                    if (map.Contains(j))
                        continue;
                   
                    var len = LengthSquared(source.Figures[i].StartPoint - target.Figures[j].StartPoint);
                    if (len < closest)
                    {
                        closest = len;
                        closestIndex = j;
                    }
                }

                map[i] = closestIndex;
            }

            for (int i = 0; i < source.Figures.Count; i++)
                MorphFigure(source.Figures[i], target.Figures[map[i]], progress);
        }

        private static double LengthSquared(Point point)
        {
            return point.X * point.X + point.Y*point.Y;
        }
        
        public static void MorphFigure(PathFigure source, PathFigure target, double progress)
        {
            var sourceSegment = (LineSegment)source.Segments[0];
            var targetSegment = (LineSegment)target.Segments[0];

            //
            // Interpolate from source to target.
            //
            if (progress >= 1)
            {
                var toX = targetSegment.Point.X;
                var toY = targetSegment.Point.Y;
                sourceSegment.Point = new Point(toX, toY);
                source.StartPoint = new Point(target.StartPoint.X, target.StartPoint.Y);
            }
            else
            {
                var fromX = sourceSegment.Point.X;
                var toX = targetSegment.Point.X;

                var fromY = sourceSegment.Point.Y;
                var toY = targetSegment.Point.Y;

                if (fromX != toX || fromY != toY)
                {
                    var x = Interpolate(fromX, toX, progress);
                    var y = Interpolate(fromY, toY, progress);
                    sourceSegment.Point = new Point(x, y);
                }

                if (source.StartPoint.X != target.StartPoint.X || 
                    source.StartPoint.Y != target.StartPoint.Y)
                {
                    var newX = Interpolate(source.StartPoint.X, target.StartPoint.X, progress);
                    var newY = Interpolate(source.StartPoint.Y, target.StartPoint.Y, progress);
                    source.StartPoint = new Point(newX, newY);
                }
            }
        }

        public static int MorphCollapse(PathFigure source, double progress)
        {
            var sourceSegment = (PolyLineSegment)source.Segments[0];

            //
            // Find Centroid
            //
            var centroid = GetCentroid(sourceSegment.Points);
            for (int i = 0; i < sourceSegment.Points.Count; i++)
            {
                var fromX = sourceSegment.Points[i].X;
                var toX = centroid.X;

                var fromY = sourceSegment.Points[i].Y;
                var toY = centroid.Y;

                var x = Interpolate(fromX, toX, progress);
                var y = Interpolate(fromY, toY, progress);

                sourceSegment.Points[i] = new Point(x, y);
            }

            var newX = Interpolate(source.StartPoint.X, centroid.X, progress);
            var newY = Interpolate(source.StartPoint.Y, centroid.Y, progress);

            source.StartPoint = new Point(newX, newY);

            if (centroid.X - newX < 0.005)
            {
                return 1;
            }

            return 0;
        }

        public static Point GetCentroid(IList<Point> nodes)
        {
            double x = 0, y = 0, area = 0, k;
            Point a, b = nodes[nodes.Count - 1];

            for (int i = 0; i < nodes.Count; i++)
            {
                a = nodes[i];

                k = a.Y * b.X - a.X * b.Y;
                area += k;
                x += (a.X + b.X) * k;
                y += (a.Y + b.Y) * k;

                b = a;
            }
            area *= 3;

            return (area == 0) ? new Point() : new Point(x /= area, y /= area);
        }

        public static double Interpolate(double from, double to, double progress)
        {
            return from + (to - from) * progress;
        }
    }

}
