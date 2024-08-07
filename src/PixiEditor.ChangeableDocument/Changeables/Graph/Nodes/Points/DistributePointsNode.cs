using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

[NodeInfo("DistributePoints")]
public class DistributePointsNode : Node
{
    private readonly Paint averageGrayscalePaint = new()
    {
        ColorFilter = Filters.AverageGrayscaleFilter
    };
    
    public override string DisplayName { get; set; } = "DISTRIBUTE_POINTS";

    public OutputProperty<PointList> Points { get; }

    public InputProperty<Surface> Probability { get; }

    public InputProperty<int> MaxPointCount { get; }

    public InputProperty<int> Seed { get; }

    public DistributePointsNode()
    {
        Points = CreateOutput(nameof(Points), "POINTS", PointList.Empty);

        Probability = CreateInput<Surface>("Probability", "PROBABILITY", null);
        MaxPointCount = CreateInput("MaxPointCount", "MAX_POINTS", 10);
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        Points.Value = Probability.Value switch
        {
            { } prop => GetPointsByProbability(prop),
            _ => GetPointsRandomly()
        };
        
        return null;
    }

    private PointList GetPointsRandomly()
    {
        var seed = Seed.Value;
        var random = new Random(seed);
        var pointCount = MaxPointCount.Value;
        var finalPoints = new PointList(pointCount)
        {
            HashValue = HashCode.Combine(Probability.Value, pointCount, seed)
        };

        for (int i = 0; i < pointCount; i++)
        {
            finalPoints.Add(new VecD(random.NextDouble(), random.NextDouble()));
        }
        
        return finalPoints;
    }

    private PointList GetPointsByProbability(Surface probability)
    {
        var size = probability.Size;
        using var probabilityImage = Surface.UsingColorType(size, ColorType.Gray8);
        probabilityImage.DrawingSurface.Canvas.DrawSurface(probability.DrawingSurface, 0, 0, averageGrayscalePaint);

        using var pixmap = probabilityImage.PeekPixels();

        var random = new Random(Seed.Value);
        var pixels = pixmap.GetPixelSpan<byte>();
        
        var rowSumCache = new int[size.Y];
        var rowColCache = new int[size.X];
        Array.Fill(rowSumCache, -1);
        
        var pointCount = MaxPointCount.Value;
        var finalPoints = new PointList(pointCount) { HashValue = 0 };
        
        for (int i = 0; i < pointCount; i++)
        {
            var xColRandom = random.Next(size.Y);
            var columnSum = GetColumnSum(xColRandom, size, pixels, rowColCache);
            
            var yRowRandom = random.Next(size.Y);
            var row = pixels.Slice(yRowRandom * size.X, size.X);
            var rowSum = GetRowSum(yRowRandom, row, rowSumCache);

            var xRandom = random.Next(rowSum);
            var yRandom = random.Next(columnSum);
            
            int counted = 0;
            int finalX = GetFinalPosition(row.Length, xRandom, row, (s, j) => s[j]);
            int finalY = GetFinalPosition(size.Y, yRandom, pixels, (s, j) => s[j * size.X + xColRandom]);

            if (finalX == -1 || finalY == -1)
            {
                continue;
            }
            
            finalPoints.Add(new VecD((double)finalX / size.X, (double)finalY / size.Y));
        }

        return finalPoints;

        static int GetFinalPosition(int size, int random, Span<byte> pixels, SpanAccessor accessor)
        {
            int counted = 0;
            int final;
            for (final = 0; final < size; final++)
            {
                counted += accessor(pixels, final);

                if (counted > random)
                {
                    //finalPoints.Add(new VecD((double)j / size.X, (double)yRowRandom / size.Y));
                    return final;
                }
            }

            return -1;
        }
        
    }
    
    delegate byte SpanAccessor(Span<byte> span, int index);

    private static int GetColumnSum(int x, VecI size, Span<byte> pixels, int[] sumCache)
    {
        int sum = sumCache[x];

        if (sum == -1)
        {
            sum = 0;
            for (int y = 0; y < size.Y; y++)
            {
                sum += pixels[size.X * y];
            }

            sumCache[x] = sum;
        }

        return sum;
    }

    private static int GetRowSum(int y, ReadOnlySpan<byte> row, int[] sumCache)
    {
        var sum = sumCache[y];

        if (sum == -1)
        {
            sum = 0;
            foreach (var value in row)
            {
                sum += value;
            }

            sumCache[y] = sum;
        }

        return sum;
    }
    
    public override Node CreateCopy() => new DistributePointsNode();
}
