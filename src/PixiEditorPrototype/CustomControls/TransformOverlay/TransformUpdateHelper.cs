using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal static class TransformUpdateHelper
{
    public static ShapeCorners? UpdateShapeFromCorner
        (Anchor targetCorner, TransformCornerFreedom freedom, ShapeCorners corners, Vector2d desiredPos)
    {
        if (!TransformHelper.IsCorner(targetCorner))
            throw new ArgumentException($"{targetCorner} is not a corner");

        if (freedom == TransformCornerFreedom.Locked)
            return corners;

        if (freedom is TransformCornerFreedom.ScaleProportionally or TransformCornerFreedom.Scale)
        {
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetCorner);
            var opposite = TransformHelper.GetOpposite(targetCorner);
            var oppositePos = TransformHelper.GetAnchorPosition(corners, opposite);

            if (freedom == TransformCornerFreedom.ScaleProportionally)
                desiredPos = desiredPos.ProjectOntoLine(targetPos, oppositePos);

            var (neighbor1, neighbor2) = TransformHelper.GetNeighboringCorners(targetCorner);
            var neighbor1Pos = TransformHelper.GetAnchorPosition(corners, neighbor1);
            var neighbor2Pos = TransformHelper.GetAnchorPosition(corners, neighbor2);

            double angle = corners.RectRotation;
            var targetTrans = (targetPos - oppositePos).Rotate(-angle);
            var neigh1Trans = (neighbor1Pos - oppositePos).Rotate(-angle);
            var neigh2Trans = (neighbor2Pos - oppositePos).Rotate(-angle);

            Vector2d delta = (desiredPos - targetPos).Rotate(-angle);

            corners = TransformHelper.UpdateCorner(corners, targetCorner,
                (targetTrans + delta).Rotate(angle) + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, neighbor1,
                (neigh1Trans + delta.Multiply(neigh1Trans.Divide(targetTrans))).Rotate(angle) + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, neighbor2,
                (neigh2Trans + delta.Multiply(neigh2Trans.Divide(targetTrans))).Rotate(angle) + oppositePos);

            return corners;
        }

        if (freedom == TransformCornerFreedom.Free)
        {
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetCorner);
            var newCorners = TransformHelper.UpdateCorner(corners, targetCorner, desiredPos);
            return newCorners.IsLegal ? newCorners : null;
        }
        throw new ArgumentException($"Freedom degree {freedom} is not supported");
    }

    public static ShapeCorners? UpdateShapeFromSide
        (Anchor targetSide, TransformSideFreedom freedom, ShapeCorners corners, Vector2d desiredPos)
    {
        if (!TransformHelper.IsSide(targetSide))
            throw new ArgumentException($"{targetSide} is not a side");

        if (freedom == TransformSideFreedom.Locked)
            return corners;

        if (freedom is TransformSideFreedom.ScaleProportionally)
        {
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetSide);
            var opposite = TransformHelper.GetOpposite(targetSide);
            var oppositePos = TransformHelper.GetAnchorPosition(corners, opposite);

            desiredPos = desiredPos.ProjectOntoLine(targetPos, oppositePos);

            Vector2d thing = targetPos - oppositePos;
            thing = Vector2d.FromAngleAndLength(thing.Angle, 1 / thing.Length);
            double scalingFactor = (desiredPos - oppositePos) * thing;
            if (!double.IsNormal(scalingFactor))
                return corners;

            corners.TopLeft = (corners.TopLeft - oppositePos) * scalingFactor + oppositePos;
            corners.BottomRight = (corners.BottomRight - oppositePos) * scalingFactor + oppositePos;
            corners.TopRight = (corners.TopRight - oppositePos) * scalingFactor + oppositePos;
            corners.BottomLeft = (corners.BottomLeft - oppositePos) * scalingFactor + oppositePos;

            if (scalingFactor < 0)
            {
                corners.TopLeft = corners.TopLeft.ReflectAcrossLine(targetPos, oppositePos);
                corners.BottomRight = corners.BottomRight.ReflectAcrossLine(targetPos, oppositePos);
                corners.TopRight = corners.TopRight.ReflectAcrossLine(targetPos, oppositePos);
                corners.BottomLeft = corners.BottomLeft.ReflectAcrossLine(targetPos, oppositePos);
            }

            return corners;
        }

        if (freedom is TransformSideFreedom.Shear or TransformSideFreedom.Stretch or TransformSideFreedom.Free)
        {
            var (side1, side2) = TransformHelper.GetCornersOnSide(targetSide);
            var side1Pos = TransformHelper.GetAnchorPosition(corners, side1);
            var side2Pos = TransformHelper.GetAnchorPosition(corners, side2);
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetSide);

            var opposite = TransformHelper.GetOpposite(targetSide);
            var oppPos = TransformHelper.GetAnchorPosition(corners, opposite);

            if (freedom == TransformSideFreedom.Shear)
                desiredPos = desiredPos.ProjectOntoLine(side1Pos, side2Pos);
            else if (freedom == TransformSideFreedom.Stretch)
                desiredPos = desiredPos.ProjectOntoLine(targetPos, oppPos);

            var delta = desiredPos - targetPos;
            var newCorners = TransformHelper.UpdateCorner(corners, side1, side1Pos + delta);
            newCorners = TransformHelper.UpdateCorner(newCorners, side2, side2Pos + delta);

            return newCorners.IsLegal ? newCorners : null;
        }
        throw new ArgumentException($"Freedom degree {freedom} is not supported");
    }

    public static ShapeCorners UpdateShapeFromRotation(ShapeCorners corners, Vector2d origin, double angle)
    {
        corners.TopLeft = corners.TopLeft.Rotate(angle, origin);
        corners.TopRight = corners.TopRight.Rotate(angle, origin);
        corners.BottomLeft = corners.BottomLeft.Rotate(angle, origin);
        corners.BottomRight = corners.BottomRight.Rotate(angle, origin);
        return corners;
    }
}
