using Drawie.Numerics;

namespace PixiEditor.Helpers;

public static class GeometryHelper
{
    public static VecD Get45IncrementedPosition(VecD startPos, VecD curPos)
    {
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

    public static VecD Get45IncrementedPositionAlignedWithPixelCenters(VecD startPos, VecD curPos)
    {
        VecD snapped = Get45IncrementedPosition(startPos.Floor() + new VecD(0.5, 0.5), curPos); 
        
        // in the 45 degree case `snapped` can end up directly at the intersection of 4 pixels, which is ambiguous
        if ((snapped - snapped.Round()).LengthSquared < 0.01)
        {
            snapped -= (curPos - startPos).Signs() * 0.5;
        }
        
        return snapped.Floor() + new VecD(0.5, 0.5);
    }
}
