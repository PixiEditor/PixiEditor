using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering.ContextData;

public record struct PointerInfo
{
    public VecD PositionOnCanvas { get; }
    public float Pressure { get; }
    public float Twist { get; }
    public VecD Tilt { get; }
    public VecD MovementDirection { get; }
    public double Rotation { get; }

    public PointerInfo(VecD positionOnCanvas, float pressure, float twist, VecD tilt, VecD movementDirection)
    {
        PositionOnCanvas = positionOnCanvas;
        Pressure = pressure;
        Twist = twist;
        Tilt = tilt;
        MovementDirection = movementDirection;
        Rotation = Math.Atan2(movementDirection.Y, movementDirection.X);
    }
}
