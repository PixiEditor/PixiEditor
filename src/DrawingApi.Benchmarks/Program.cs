using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using DrawingApi.Benchmarks;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SkiaSharp;

public static class Program
{
    public static IWindow window;
    public static void Main()
    {
        WindowOptions options = WindowOptions.Default with { IsVisible = false };

        window = Window.Create(options);

        GRContext GrContext;
        GL gl;

        window.Load += () =>
        {
            gl = GL.GetApi(window);
            GrContext = GRContext.CreateGl();

            DrawingBackendApi.SetupBackend(new SkiaDrawingBackend(GrContext));

            Stopwatch stopwatch = new Stopwatch();
            
            SurfaceBenchmarks cpuBenchmarks = new SurfaceBenchmarks();
            cpuBenchmarks.GraphicsContext = GraphicsContext.CPU;
            
            stopwatch.Start();
            var cpuResult = cpuBenchmarks.MergeTwoSurfaces();
            stopwatch.Stop();
            
            Console.WriteLine($"Merge CPU: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            SurfaceBenchmarks glBenchmarks = new SurfaceBenchmarks();
            glBenchmarks.GraphicsContext = GraphicsContext.OpenGL;
            
            stopwatch.Restart();
            var result = glBenchmarks.MergeTwoSurfaces();
            stopwatch.Stop();
            
            Console.WriteLine($"Merge OpenGL: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            var toSnapshotCpu = cpuBenchmarks.CreateDrawingSurface(new VecI(10000, 10000));
            toSnapshotCpu.Canvas.DrawRect(0, 0, 10000, 10000, new Paint() { Color = new Color(255, 0, 0) });

            stopwatch.Restart();
            var cpuSnapshot = cpuBenchmarks.Snapshot(toSnapshotCpu);
            stopwatch.Stop();
            
            Console.WriteLine($"CPU Snapshot: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            var toSnapshotGl = glBenchmarks.CreateDrawingSurface(new VecI(10000, 10000));
            toSnapshotGl.Canvas.DrawRect(0, 0, 10000, 10000, new Paint() { Color = new Color(255, 0, 0) });
            
            stopwatch.Restart();
            var glSnapshot = glBenchmarks.Snapshot(toSnapshotGl);
            stopwatch.Stop();
            
            Console.WriteLine($"OpenGL Snapshot: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            stopwatch.Restart();
            var cpuPixmap = cpuSnapshot.PeekPixels();
            stopwatch.Stop();
            
            Console.WriteLine($"CPU Peek Pixels: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            stopwatch.Restart();
            var glPixmap = glSnapshot.PeekPixels();
            stopwatch.Stop();
            
            Console.WriteLine($"OpenGL Peek Pixels: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            stopwatch.Restart();
            cpuBenchmarks.PickColor(cpuPixmap, 5000, 5000);
            stopwatch.Stop();
            
            Console.WriteLine($"CPU Pick Color: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            stopwatch.Restart();
            glBenchmarks.PickColor(glPixmap, 5000, 5000);
            stopwatch.Stop();
            
            Console.WriteLine($"OpenGL Pick Color: {stopwatch.Elapsed.TotalMicroseconds} micro seconds");
            
            window.Close();
        };

        window.Run();
    }
    
    public static void SaveToDesktop(DrawingSurface surface, string fileName)
    {
        using Image img = surface.Snapshot();
        using var data = img.Encode();
        System.IO.File.WriteAllBytes(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName), data.AsSpan().ToArray());
    }
}
