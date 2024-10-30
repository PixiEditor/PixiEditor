using System;
using Drawie.Numerics;
using Xunit;

namespace ChunkyImageLibTest;
public class RectITests
{
    [Fact]
    public void EmptyConstructor_Call_ResultsInZeroVec()
    {
        RectI rect = new RectI();
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Right);
        Assert.Equal(0, rect.Top);
        Assert.Equal(0, rect.Bottom);
    }

    [Fact]
    public void RegularConstructor_WithBasicArgs_Works()
    {
        RectI rect = new RectI(800, 600, 200, 300);
        Assert.Equal(800, rect.Left);
        Assert.Equal(600, rect.Top);
        Assert.Equal(800 + 200, rect.Right);
        Assert.Equal(600 + 300, rect.Bottom);
    }

    [Fact]
    public void FromTwoPoints_DiagonalsCombinations_ReturnsStandardizedRects()
    {
        RectI refR = new RectI(3, 4, 8 - 3, 9 - 4);
        Span<RectI> rects = stackalloc RectI[] 
        {
            RectI.FromTwoPoints(new VecI(3, 4), new VecI(8, 9)),
            RectI.FromTwoPoints(new VecI(8, 9), new VecI(3, 4)),
            RectI.FromTwoPoints(new VecI(8, 9), new VecI(3, 4)),
            RectI.FromTwoPoints(new VecI(8, 9), new VecI(3, 4)),
        };
        foreach (var rect in rects)
        {
            Assert.Equal(
                (refR.Left, refR.Top, refR.Right, refR.Bottom),
                (rect.Left, rect.Top, rect.Right, rect.Bottom));
        }
    }

    [Fact]
    public void Properties_OfStandardRectangle_ReturnCorrectValues()
    {
        RectI r = new(new VecI(2, 3), new VecI(4, 5));
        Assert.Equal(2, r.Left);
        Assert.Equal(3, r.Top);
        Assert.Equal(2 + 4, r.Right);
        Assert.Equal(3 + 5, r.Bottom);

        Assert.Equal(r.Left, r.X);
        Assert.Equal(r.Top, r.Y);
        Assert.Equal(new VecI(r.Left, r.Top), r.Pos);
        Assert.Equal(new VecI(r.Right - r.Left, r.Bottom - r.Top), r.Size);

        Assert.Equal(new VecI(r.Left, r.Bottom), r.BottomLeft);
        Assert.Equal(new VecI(r.Right, r.Bottom), r.BottomRight);
        Assert.Equal(new VecI(r.Left, r.Top), r.TopLeft);
        Assert.Equal(new VecI(r.Right, r.Top), r.TopRight);

        Assert.Equal(r.Size.X, r.Width);
        Assert.Equal(r.Size.Y, r.Height);

        Assert.False(r.IsZeroArea);
    }

    [Fact]
    public void PropertySetters_SetPlainValues_UpdateSidesCorrectly()
    {
        RectI r = new();
        // left, top, right bottom
        (r.Left, r.Top, r.Right, r.Bottom) = (2, 3, 6, 8);
        Assert.Equal((2, 3, 6, 8), (r.Left, r.Top, r.Right, r.Bottom));

        // x, y
        (r.X, r.Y) = (4, 5);
        Assert.Equal((4, 5), (r.Left, r.Top));

        // pos
        var oldSize = new VecI(r.Right - r.Left, r.Bottom - r.Top);
        r.Pos = new VecI(5, 6);
        var newSize = new VecI(r.Right - r.Left, r.Bottom - r.Top);
        Assert.Equal((5, 6), (r.Left, r.Top));
        Assert.Equal(oldSize, newSize);

        // size
        var oldPos = r.Pos;
        r.Size = new(18, 14);
        var newPos = r.Pos;
        Assert.Equal(oldPos, newPos);
        Assert.Equal((18, 14), (r.Right - r.Left, r.Bottom - r.Top));

        // corners
        r.BottomLeft = new VecI(-13, -14);
        Assert.Equal((-13, -14), (r.Left, r.Bottom));
        r.BottomRight = new VecI(46, -12);
        Assert.Equal((46, -12), (r.Right, r.Bottom));
        r.TopLeft = new VecI(-46, 24);
        Assert.Equal((-46, 24), (r.Left, r.Top));
        r.TopRight = new VecI(100, 101);
        Assert.Equal((100, 101), (r.Right, r.Top));

        // width, height
        var oldPos2 = r.Pos;
        (r.Width, r.Height) = (1, 2);
        var newPos2 = r.Pos;
        Assert.Equal(oldPos2, newPos2);
        Assert.Equal((1, 2), (r.Right - r.Left, r.Bottom - r.Top));
    }

    [Fact]
    public void IsZeroArea_NormalRectangles_ReturnsFalse()
    {
        Assert.False(new RectI(new(5, 6), new VecI(1, 1)).IsZeroArea);
        Assert.False(new RectI(new(-5, -6), new VecI(-1, -1)).IsZeroArea);
    }

    [Fact]
    public void IsZeroArea_ZeroAreaRectangles_ReturnsFalse()
    {
        Assert.True(new RectI(new(5, 6), new VecI(0, 10)).IsZeroArea);
        Assert.True(new RectI(new(-5, -6), new VecI(10, 0)).IsZeroArea);
        Assert.True(new RectI(new(-5, -6), new VecI(0, 0)).IsZeroArea);
    }

    [Fact]
    public void Standardize_StandardRects_RemainUnchanged()
    {
        var rect1 = new RectI(new(4, 5), new(1, 1));
        Assert.Equal(rect1, rect1.Standardize());
        var rect2 = new RectI(new(-4, -5), new(1, 1));
        Assert.Equal(rect2, rect2.Standardize());
    }

    [Fact]
    public void Standardize_NonStandardRects_BecomeStandard()
    {
        var rect1 = new RectI(4, 5, -1, -1);
        Assert.Equal(new RectI(3, 4, 1, 1), rect1.Standardize());
        var rect2 = new RectI(-4, -5, -1, 1);
        Assert.Equal(new RectI(-5, -5, 1, 1), rect2.Standardize());
        var rect3 = new RectI(-4, -5, 1, -1);
        Assert.Equal(new RectI(-4, -6, 1, 1), rect3.Standardize());
    }

    [Fact]
    public void ReflectX_BasicRect_ReturnsReflected()
    {
        var rect = new RectI(4, 5, 6, 7);
        Assert.Equal(new RectI(-4, 5, 6, 7), rect.ReflectX(3));
    }

    [Fact]
    public void ReflectY_BasicRect_ReturnsReflected()
    {
        var rect = new RectI(4, 5, 6, 7);
        Assert.Equal(new RectI(4, -6, 6, 7), rect.ReflectY(3));
    }

    [Fact]
    public void Inflate_BasicRect_ReturnsInflated()
    {
        var rect = new RectI(4, 5, 6, 7);
        var infInt = rect.Inflate(2);
        var infVec = rect.Inflate(2, 3);
        Assert.Equal(new RectI(2, 3, 10, 11), infInt);
        Assert.Equal(new RectI(2, 2, 10, 13), infVec);
    }

    [Fact]
    public void AspectFit_FitPortraitIntoLandscape_FitsCorrectly()
    {
        RectI landscape = new(-1, 4, 5, 3);
        RectI portrait = new(32, -41, 41, 41 * 3);
        RectI fitted = landscape.AspectFit(portrait);
        Assert.Equal(new RectI(1, 4, 1, 3), fitted);
    }

    [Fact]
    public void AspectFit_FitLandscapeIntoPortrait_FitsCorrectly()
    {
        RectI portrait = new(1, -10, 7, 15);
        RectI landscape = new(-314, 1592, 23 * 7, 23 * 3);
        RectI fitted = portrait.AspectFit(landscape);
        Assert.Equal(new RectI(1, -4, 7, 3), fitted);
    }

    [Fact]
    public void ContainsInclusive_BasicRect_DeterminedCorrectly()
    {
        RectI rect = new(5, 4, 10, 11);
        Assert.True(rect.ContainsInclusive(5, 4));
        Assert.True(rect.ContainsInclusive(5 + 10, 4 + 11));
        Assert.True(rect.ContainsInclusive(5, 4 + 2));
        Assert.True(rect.ContainsInclusive(5 + 2, 4));
        Assert.True(rect.ContainsInclusive(6, 5));

        Assert.False(rect.ContainsInclusive(0, 0));
        Assert.False(rect.ContainsInclusive(6, 80));
        Assert.False(rect.ContainsInclusive(80, 6));
        Assert.False(rect.ContainsInclusive(5 + 11, 4 + 10));
    }

    [Fact]
    public void ContainsExclusive_BasicRect_DeterminedCorrectly()
    {
        RectI rect = new(5, 4, 10, 11);
        Assert.False(rect.ContainsExclusive(5, 4));
        Assert.False(rect.ContainsExclusive(5 + 10, 4 + 11));
        Assert.False(rect.ContainsExclusive(5, 4 + 2));
        Assert.False(rect.ContainsExclusive(5 + 2, 4));

        Assert.True(rect.ContainsExclusive(6, 5));
        Assert.True(rect.ContainsExclusive(5 + 9, 4 + 10));

        Assert.False(rect.ContainsExclusive(0, 0));
        Assert.False(rect.ContainsExclusive(6, 80));
        Assert.False(rect.ContainsExclusive(80, 6));
        Assert.False(rect.ContainsExclusive(5 + 11, 4 + 10));
    }

    [Fact]
    public void ContainsPixel_BasicRect_DeterminedCorrectly()
    {
        RectI rect = new RectI(960, 540, 1920, 1080);
        Assert.True(rect.ContainsPixel(960, 540));
        Assert.True(rect.ContainsPixel(1920 - 1, 1080 - 1));
        Assert.True(rect.ContainsPixel(960 + 960 / 2, 540 + 540 / 2));

        Assert.False(rect.ContainsPixel(960 - 1, 540 - 1));
        Assert.False(rect.ContainsPixel(960 + 1920, 540 + 1080));
        Assert.False(rect.ContainsPixel(960 + 960, 1080 + 540));
    }

    [Fact]
    public void IntersectsWithInclusive_BasicRects_ReturnsTrue()
    {
        RectI rect = new RectI(960, 540, 1920, 1080);
        Span<RectI> rects = stackalloc RectI[] 
        {
            rect.Offset(1920, 1080),
            rect.Offset(-1920, 0).Inflate(-1).Offset(1, 0),
            rect.Offset(0, 1080).Inflate(-1).Offset(0, -1),
            rect.Inflate(-1),
            rect.Inflate(1),
        };
        foreach (var testRect in rects)
            Assert.True(rect.IntersectsWithInclusive(testRect));
    }

    [Fact]
    public void IntersectsWithInclusive_BasicRects_ReturnsFalse()
    {
        RectI rect = new RectI(960, 540, 1920, 1080);
        Span<RectI> rects = stackalloc RectI[] 
        {
            rect.Offset(1921, 1080),
            rect.Offset(-1921, 0).Inflate(-1).Offset(1, 0),
            rect.Offset(0, 1081).Inflate(-1).Offset(0, -1)
        };
        foreach (var testRect in rects)
            Assert.False(rect.IntersectsWithInclusive(testRect));
    }

    [Fact]
    public void IntersectsWithExclusive_BasicRects_ReturnsTrue()
    {
        RectI rect = new RectI(960, 540, 1920, 1080);
        Span<RectI> rects = stackalloc RectI[] 
        {
            rect.Offset(1920 - 1, 1080 - 1),
            rect.Offset(-1920, 0).Inflate(-1).Offset(2, 0),
            rect.Offset(0, 1080).Inflate(-1).Offset(0, -2),
            rect.Inflate(-1),
            rect.Inflate(1),
        };
        foreach (var testRect in rects)
            Assert.True(rect.IntersectsWithExclusive(testRect));
    }

    [Fact]
    public void IntersectsWithExclusive_BasicRects_ReturnsFalse()
    {
        RectI rect = new RectI(960, 540, 1920, 1080);
        Span<RectI> rects = stackalloc RectI[] 
        {
            rect.Offset(1920, 1080),
            rect.Offset(-1920, 0).Inflate(-1).Offset(1, 0),
            rect.Offset(0, 1080).Inflate(-1).Offset(0, -1),
            rect.Offset(1921, 1080),
            rect.Offset(-1921, 0).Inflate(-1).Offset(1, 0),
            rect.Offset(0, 1081).Inflate(-1).Offset(0, -1)
        };
        foreach (var testRect in rects)
            Assert.False(rect.IntersectsWithExclusive(testRect));
    }

    [Fact]
    public void Intersect_IntersectingRectangles_ReturnsIntersection()
    {
        Assert.Equal(
            new RectI(400, 300, 400, 300),
            new RectI(400, 300, 800, 600).Intersect(new RectI(0, 0, 800, 600)));
    }

    [Fact]
    public void Intersect_NonIntersectingRectangles_ReturnsEmpty()
    {
        Assert.Equal(
            RectI.Empty,
            new RectI(-123, -456, 78, 10).Intersect(new RectI(123, 456, 789, 101)));
    }

    [Fact]
    public void Union_BasicRectangles_ReturnsUnion()
    {
        var rect1 = new RectI(4, 5, 1, 1);
        var rect2 = new RectI(-4, -5, 1, 1);
        Assert.Equal(new RectI(-4, -5, 9, 11), rect1.Union(rect2));
        Assert.Equal(new RectI(-4, -5, 9, 11), rect2.Union(rect1));
    }
}
