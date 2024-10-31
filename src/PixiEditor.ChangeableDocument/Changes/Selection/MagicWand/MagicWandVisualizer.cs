using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Selection.MagicWand;

internal class MagicWandVisualizer
{
    public string CurrentContext { get; set; } = "";
    public string FilePath { get; }
    private Paint drawingPaint;
    private Paint replacementPaint;
    public List<Step> Steps = new List<Step>();

    public MagicWandVisualizer(string filePath)
    {
        drawingPaint = new Paint();
        drawingPaint.BlendMode = BlendMode.Src;

        replacementPaint = new Paint();
        replacementPaint.BlendMode = BlendMode.Src;
        replacementPaint.ColorFilter = ColorFilter.CreateBlendMode(Colors.Black, BlendMode.ColorBurn);

        FilePath = filePath;
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
    }

    public void GenerateVisualization(int originalHeight, int originalWidth, int width, int height)
    {
        Directory.EnumerateFiles(FilePath).ToList().ForEach(File.Delete);

        Image? previousImage = null;

        for (int i = 0; i < Steps.Count; i++)
        {
            Step step = Steps[i];
            var surface = DrawingSurface.Create(new ImageInfo(width, height));
            surface.Canvas.Clear(Colors.White);
            if (previousImage != null)
            {
                surface.Canvas.DrawImage(
                    previousImage,
                    RectD.Create(VecI.Zero, new VecI(previousImage.Width, previousImage.Height)), 
                    replacementPaint);
            }

            var scaledStart = new VecI(step.Start.X * (width / originalWidth), step.Start.Y * (height / originalHeight));
            var scaledEnd = new VecI(step.End.X * (width / originalWidth), step.End.Y * (height / originalHeight));

            DrawArrow(surface, scaledStart, scaledEnd, 2, step.Type == StepType.Add ? Colors.Green : Colors.Red);

            using (FileStream stream = new FileStream(Path.Join(FilePath, $"Frame {i}_{CurrentContext}.png"), FileMode.Create))
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

internal class Step
{
    public VecI Start { get; set; }
    public VecI End { get; set; }
    public StepType Type { get; set; }

    public Step(MagicWandHelper.Line line, StepType type = StepType.Add)
    {
        Start = line.Start;
        End = line.End;
        Type = type;
    }
}

internal enum StepType
{
    Add,
    CancelLine
}
