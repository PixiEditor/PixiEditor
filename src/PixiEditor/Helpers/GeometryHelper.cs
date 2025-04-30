using Drawie.Numerics;

namespace PixiEditor.Helpers;

public static class GeometryHelper
{
    public static VecD Get45IncrementedPosition(VecD startPos, VecD curPos)
    {

        VecD delta = curPos - startPos;
        VecD signs = delta.Signs();
        VecD offset = new VecD(0).Multiply(signs);

        VecD[] positions =
        [
            curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 1)),
            curPos.ProjectOntoLine(startPos, startPos + new VecD(1, -1)),
            curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 0)),
            curPos.ProjectOntoLine(startPos, startPos + new VecD(0, 1))
        ];

        VecD closest = positions[0];
        double minDistSq = double.MaxValue;
        foreach (var pos in positions)
        {
            double distSq = (pos - curPos).LengthSquared;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = pos;
            }
        }

        return closest;
    }

    public static VecI Get45IncrementedPositionAligned(VecD startPos, VecD curPos)
    {
        Span<VecI> positions =
        [
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, -1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(1, 0)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round(),
            (VecI)(curPos.ProjectOntoLine(startPos, startPos + new VecD(0, 1)) -
                   new VecD(0.25).Multiply((curPos - startPos).Signs())).Round()
        ];

        VecI max = positions[0];
        double maxLength = double.MaxValue;
        foreach (var pos in positions)
        {
            double length = (pos - curPos).LengthSquared;
            if (length < maxLength)
            {
                maxLength = length;
                max = pos;
            }
        }

        return max;
    }
}
