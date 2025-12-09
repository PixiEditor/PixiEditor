using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering.ContextData;

public record struct PointerInfo
{
    public bool IsLeftButtonPressed { get; set; }
    public bool IsRightButtonPressed { get; set; }
    public VecD PositionOnCanvas { get; set; }
    public float Pressure { get; set; }
    public float Twist { get; set; }
    public VecD Tilt { get; set; }
    public VecD MovementDirection { get; set; }
    public double Rotation { get; set; }

    public PointerInfo(VecD positionOnCanvas, float pressure, float twist, VecD tilt, VecD movementDirection, bool isLeftButtonPressed, bool isRightButtonPressed)
    {
        PositionOnCanvas = positionOnCanvas;
        Pressure = pressure;
        Twist = twist;
        Tilt = tilt;
        MovementDirection = movementDirection;
        IsLeftButtonPressed = isLeftButtonPressed;
        IsRightButtonPressed = isRightButtonPressed;
        Rotation = Math.Atan2(movementDirection.Y, movementDirection.X);
    }
}
