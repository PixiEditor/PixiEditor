using PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Debugging;

public class MagicWandVisualizer
{
    public string FilePath { get; }
    private Paint drawingPaint;
    private Paint replacementPaint;
    public List<FloodFillHelper.Line> Steps = new List<FloodFillHelper.Line>();
    
    public MagicWandVisualizer(string filePath)
    {
        drawingPaint = new Paint();
        drawingPaint.BlendMode = BlendMode.Src;
        
        replacementPaint = new Paint();
        replacementPaint.BlendMode = BlendMode.Src;
        replacementPaint.ColorFilter = ColorFilter.CreateBlendMode(Colors.Black, BlendMode.ColorBurn);
        
        FilePath = filePath;
        if(!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        
        Directory.EnumerateFiles(filePath).ToList().ForEach(File.Delete);
    }

    public void GenerateVisualization(int originalHeight, int originalWidth, int width, int height)
    {
        DrawingSurface surface = DrawingSurface.Create(new ImageInfo(width, height));
        surface.Canvas.Clear(Colors.White);
        Image previousImage = surface.Snapshot();
        VecI scaledStart = new VecI(Steps[0].Start.X * (width / originalWidth), Steps[0].Start.Y * (height / originalHeight));
        VecI scaledEnd = new VecI(Steps[0].End.X * (width / originalWidth), Steps[0].End.Y * (height / originalHeight));
        
        DrawArrow(surface, scaledStart, scaledEnd, 2, Colors.Green);
        using (FileStream stream = new FileStream(Path.Join(FilePath, "Frame 1.png"), FileMode.Create))
        {
            surface.Snapshot().Encode().SaveTo(stream);
        }

        for (int i = 1; i < Steps.Count; i++)
        {
            surface = DrawingSurface.Create(new ImageInfo(width, height));
            surface.Canvas.Clear(Colors.White);
            surface.Canvas.DrawImage(previousImage, RectD.Create(VecI.Zero, new VecI(previousImage.Width, previousImage.Height)), replacementPaint);
            
            scaledStart = new VecI(Steps[i].Start.X * (width / originalWidth), Steps[i].Start.Y * (height / originalHeight));
            scaledEnd = new VecI(Steps[i].End.X * (width / originalWidth), Steps[i].End.Y * (height / originalHeight));
            
            DrawArrow(surface, scaledStart, scaledEnd, 2, Colors.Green);
            
            using (FileStream stream = new FileStream(Path.Join(FilePath, $"Frame {i}.png"), FileMode.Create))
            {
                surface.Snapshot().Encode().SaveTo(stream);
            }
            
            previousImage = surface.Snapshot();
        }
        
        Steps.Clear();
    }

    private void DrawArrow(DrawingSurface surface, VecI start, VecI end, int thickness, Color color)
    {
        drawingPaint.Color = color;
        drawingPaint.StrokeWidth = thickness;
        surface.Canvas.DrawLine(start, end, drawingPaint);
        
        // Draw arrow head
        
        VecI direction = end - start;
        VecI perpendicular = new VecI(-direction.Y, direction.X);
        
        VecI arrowHead1 = end - (VecI)direction.Normalized() * 10 + (VecI)perpendicular.Normalized() * 5;
        VecI arrowHead2 = end - (VecI)direction.Normalized() * 10 - (VecI)perpendicular.Normalized() * 5;
        
        surface.Canvas.DrawLine(end, arrowHead1, drawingPaint);
        surface.Canvas.DrawLine(end, arrowHead2, drawingPaint);
    }
}
