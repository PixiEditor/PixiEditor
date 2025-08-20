using System.Buffers;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Nodes;

public sealed class NodeZoneViewModel : NodeFrameViewModelBase
{
    public INodeHandler Start { get; }
    
    public INodeHandler End { get; }

    public NodeZoneViewModel(Guid id, string internalName, INodeHandler start, INodeHandler end) : base(id,
        [start, end])
    {
        InternalName = internalName;

        this.Start = start.Metadata.IsPairNodeStart ? start : end;
        this.End = start.Metadata.IsPairNodeStart ? end : start;

        CalculateBounds();
    }

    protected override void CalculateBounds()
    {
        if (Nodes.Count == 0)
        {
            return;
        }

        var points = GetBoundPoints();

        Geometry = BuildRoundedHullGeometry(points, 25);
    }

    private static StreamGeometry BuildRoundedHullGeometry(List<VecD> points, double cornerRadius)
    {
        const double startBoostDeg = 100;
        const double maxBoost = 2.5;

        var span = CollectionsMarshal.AsSpan(points);

        var pool = ArrayPool<VecD>.Shared;
        var hullBuf = pool.Rent(Math.Max(3, span.Length));

        try
        {
            var hullCount = ConvexHull(span, hullBuf.AsSpan());
            var hull = hullBuf.AsSpan(0, hullCount);

            var geometry = new StreamGeometry();
            if (hull.IsEmpty) return geometry;

            using var ctx = geometry.Open();
            
            if (hull.Length <= 2 || cornerRadius <= 0)
            {
                ctx.BeginFigure(new Point(hull[0].X, hull[0].Y), isFilled: true);
                for (var i = 1; i < hull.Length; i++)
                    ctx.LineTo(new Point(hull[i].X, hull[i].Y));
                ctx.EndFigure(isClosed: true);
                return geometry;
            }

            var n = hull.Length;

            var enter = n <= 256 ? stackalloc VecD[n] : pool.Rent(n).AsSpan(0, n);
            var exit = n <= 256 ? stackalloc VecD[n] : pool.Rent(n).AsSpan(0, n);
            var rented = n > 256;

            try
            {
                for (var i = 0; i < n; i++)
                {
                    var prev = hull[(i - 1 + n) % n];
                    var current = hull[i];
                    var next = hull[(i + 1) % n];

                    var directionIn = (current - prev).Normalize();
                    var directionOut = (next - current).Normalize();
                    var lenIn = (prev - current).Length;
                    var lenOut = (current - next).Length;

                    var a = (prev - current).Normalize();
                    var b = (next - current).Normalize();
                    var dot = Math.Clamp(a.X * b.X + a.Y * b.Y, -1, 1);
                    var theta = Math.Acos(dot); // radians

                    // Boost wide angles a bit (same curve, fewer ops)
                    var thetaDeg = theta * (180.0 / Math.PI);
                    var tNorm = Math.Clamp((thetaDeg - startBoostDeg) / (180.0 - startBoostDeg), 0, 1);
                    var s = tNorm * tNorm * (3 - 2 * tNorm); // smoothstep
                    var radiusHere = cornerRadius * (1 + (maxBoost - 1) * s);

                    var t = (theta > 1e-6) ? radiusHere / Math.Tan(theta / 2.0) : 0;
                    var tMax = Math.Min(lenIn, lenOut) * 0.5;
                    t = Math.Min(t, tMax);

                    if (t <= 1e-6)
                    {
                        enter[i] = current;
                        exit[i] = current;
                    }
                    else
                    {
                        enter[i] = current + directionIn * -t;
                        exit[i] = current + directionOut * t;
                    }
                }

                ctx.BeginFigure(new Point(enter[0].X, enter[0].Y), isFilled: true);

                for (var i = 0; i < n; i++)
                {
                    ctx.QuadraticBezierTo(
                        new Point(hull[i].X, hull[i].Y),
                        new Point(exit[i].X, exit[i].Y));

                    var nextEnter = enter[(i + 1) % n];
                    ctx.LineTo(new Point(nextEnter.X, nextEnter.Y));
                }

                ctx.EndFigure(isClosed: true);
            }
            finally
            {
                if (rented)
                {
                    pool.Return(
                        MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(enter), n).ToArray());

                    pool.Return(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(exit), n)
                        .ToArray());
                }
            }

            return geometry;
        }
        finally
        {
            pool.Return(hullBuf);
        }
    }

    private static int ConvexHull(ReadOnlySpan<VecD> input, Span<VecD> hull)
    {
        var n = input.Length;
        if (n <= 1)
        {
            if (n == 1) hull[0] = input[0];
            return n;
        }

        var pool = ArrayPool<VecD>.Shared;
        var pts = pool.Rent(n);

        try
        {
            input.CopyTo(pts);
            Array.Sort(pts, 0, n, VecDComparer.Instance);

            var m = 0;
            for (var i = 0; i < n; i++)
            {
                if (m == 0 || !pts[i].Equals(pts[m - 1]))
                    pts[m++] = pts[i];
            }

            if (m <= 1)
            {
                if (m == 1) hull[0] = pts[0];
                return m;
            }

            var k = 0;

            for (var i = 0; i < m; i++)
            {
                while (k >= 2 && (hull[k - 1] - hull[k - 2]).Cross(pts[i] - hull[k - 2]) <= 0)
                    k--;
                hull[k++] = pts[i];
            }

            var t = k + 1;
            for (var i = m - 2; i >= 0; i--)
            {
                while (k >= t && (hull[k - 1] - hull[k - 2]).Cross(pts[i] - hull[k - 2]) <= 0)
                    k--;
                hull[k++] = pts[i];
            }

            return k - 1;
        }
        finally
        {
            pool.Return(pts);
        }
    }

    private sealed class VecDComparer : IComparer<VecD>
    {
        public static readonly VecDComparer Instance = new();

        public int Compare(VecD a, VecD b)
        {
            var cx = a.X.CompareTo(b.X);
            return cx != 0 ? cx : a.Y.CompareTo(b.Y);
        }
    }

    private List<VecD> GetBoundPoints()
    {
        var list = new List<VecD>(Nodes.Count * 4);

        const int defaultXOffset = 30;
        const int defaultYOffset = 45;

        foreach (var node in Nodes)
        {
            var pos = node.PositionBindable;
            var size = new VecD(node.UiSize.Size.Width, node.UiSize.Size.Height);

            if (node == Start)
            {
                var twoThirdsX = size.X * (2.0 / 3.0);

                list.Add(pos + new VecD(twoThirdsX, -defaultYOffset));
                list.Add(pos + new VecD(twoThirdsX, defaultYOffset + size.Y));

                list.Add(pos + new VecD(size.X + defaultXOffset, -defaultYOffset));
                list.Add(pos + new VecD(size.X + defaultXOffset, defaultYOffset + size.Y));
                continue;
            }

            if (node == End)
            {
                var oneThirdX = size.X / 3.0;

                list.Add(pos + new VecD(oneThirdX, -defaultYOffset));
                list.Add(pos + new VecD(oneThirdX, defaultYOffset + size.Y));

                list.Add(pos + new VecD(-defaultXOffset, -defaultYOffset));
                list.Add(pos + new VecD(-defaultXOffset, defaultYOffset + size.Y));
                continue;
            }

            var right = defaultXOffset + size.X;
            var bottom = defaultYOffset + size.Y;

            list.Add(pos + new VecD(-defaultXOffset, -defaultYOffset));
            list.Add(pos + new VecD(right, -defaultYOffset));
            list.Add(pos + new VecD(-defaultXOffset, bottom));
            list.Add(pos + new VecD(right, bottom));
        }

        return list;
    }
}
