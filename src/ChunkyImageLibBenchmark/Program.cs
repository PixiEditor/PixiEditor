using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using SkiaSharp;
using System.Diagnostics;


for (int i = 0; i < 10; i++)
{
    Benchmark();
}

int count = 10000;
double totalFirst = 0;
double totalSecond = 0;
for (int i = 0; i < count; i++)
{
    (double first, double second) = Benchmark();
    totalFirst += first;
    totalSecond += second;
}

Console.WriteLine($"took {totalFirst / count} ms first, then {totalSecond / count} ms");
Console.ReadKey();

(double first, double second) Benchmark()
{
    using ChunkyImage image = new(new(1024, 1024), ColorType.RgbaF16);
    image.DrawRectangle(new(new(0, 0), new(1024, 1024), 10, SKColors.Black, SKColors.Bisque));

    Stopwatch sw = Stopwatch.StartNew();
    for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            //image.GetLatestChunk(new(i, j), ChunkyImageLib.DataHolders.ChunkResolution.Full);
        }
    }
    sw.Stop();
    double first = sw.ElapsedTicks / (double)Stopwatch.Frequency * 1000;

    sw = Stopwatch.StartNew();
    for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            //image.GetLatestChunk(new(i, j), ChunkyImageLib.DataHolders.ChunkResolution.Full);
        }
    }
    sw.Stop();
    double second = sw.ElapsedTicks / (double)Stopwatch.Frequency * 1000;

    return (first, second);
}
