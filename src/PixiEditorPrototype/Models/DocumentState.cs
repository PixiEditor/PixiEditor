using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models
{
    internal class DocumentState
    {
        public Vector2d ViewportCenter { get; set; } = new(32, 32);
        public Vector2d ViewportSize { get; set; } = new(64, 64);
        public Vector2d ViewportRealSize { get; set; } = new(double.MaxValue, double.MaxValue);
        public double ViewportAngle { get; set; } = 0;
    }
}
