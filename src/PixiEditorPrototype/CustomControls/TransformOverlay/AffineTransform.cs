using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal record struct AffineTransform
{
    public AffineTransform(Vector2d center, Vector2d size, double angle)
    {
        Center = center;
        Size = size;
        Angle = angle;
    }
    public double Angle { get; }
    public Vector2d Size { get; }
    public Vector2d Center { get; }

    public Vector2d TopLeft => -(Size / 2).Rotate(Angle) + Center;
    public Vector2d TopRight => new Vector2d(Size.X / 2, -Size.Y / 2).Rotate(Angle) + Center;
    public Vector2d BottomRight => (Size / 2).Rotate(Angle) + Center;
    public Vector2d BottomLeft => new Vector2d(-Size.X / 2, Size.Y / 2).Rotate(Angle) + Center;
}
