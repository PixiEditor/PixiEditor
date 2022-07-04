namespace PixiEditor.Models.Tools.Brushes
{
    public class InterestingShapeBrush : MatrixBrush
    {
        public static readonly int[,] InterestingShapeMatrix = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        public override int[,] BrushMatrix => InterestingShapeMatrix;
    }
}