using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal static class TransformUpdateHelper
{
    public static ShapeCorners? UpdateShapeFromCorner
        (Anchor targetCorner, TransformCornerFreedom freedom, double propAngle1, double propAngle2, ShapeCorners corners, Vector2d desiredPos)
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
            {
                double correctAngle = targetCorner is Anchor.TopLeft or Anchor.BottomRight ? propAngle1 : propAngle2;
                desiredPos = desiredPos.ProjectOntoLine(oppositePos, oppositePos + Vector2d.FromAngleAndLength(correctAngle, 1));
            }

            var (leftNeighbor, rightNeighbor) = TransformHelper.GetNeighboringCorners(targetCorner);
            var leftNeighborPos = TransformHelper.GetAnchorPosition(corners, leftNeighbor);
            var rightNeighborPos = TransformHelper.GetAnchorPosition(corners, rightNeighbor);

            double angle = corners.RectRotation;
            var targetTrans = (targetPos - oppositePos).Rotate(-angle);
            var leftNeighTrans = (leftNeighborPos - oppositePos).Rotate(-angle);
            var rightNeighTrans = (rightNeighborPos - oppositePos).Rotate(-angle);

            Vector2d delta = (desiredPos - targetPos).Rotate(-angle);

            corners = TransformHelper.UpdateCorner(corners, targetCorner,
                (targetTrans + delta).Rotate(angle) + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, leftNeighbor,
                (leftNeighTrans + delta.Multiply(leftNeighTrans.Divide(targetTrans))).Rotate(angle) + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, rightNeighbor,
                (rightNeighTrans + delta.Multiply(rightNeighTrans.Divide(targetTrans))).Rotate(angle) + oppositePos);

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
        (Anchor targetSide, TransformSideFreedom freedom, double propAngle1, double propAngle2, ShapeCorners corners, Vector2d desiredPos)
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

            if (corners.IsRect)
            {
                var delta = desiredPos - targetPos;
                var center = oppositePos.Lerp(desiredPos, 0.5);

                var (leftCorn, rightCorn) = TransformHelper.GetCornersOnSide(targetSide);
                var (leftOppCorn, _) = TransformHelper.GetNeighboringCorners(leftCorn);
                var (_, rightOppCorn) = TransformHelper.GetNeighboringCorners(rightCorn);

                var leftCornPos = TransformHelper.GetAnchorPosition(corners, leftCorn);
                var rightCornPos = TransformHelper.GetAnchorPosition(corners, rightCorn);
                var leftOppCornPos = TransformHelper.GetAnchorPosition(corners, leftOppCorn);
                var rightOppCornPos = TransformHelper.GetAnchorPosition(corners, rightOppCorn);

                var (leftAngle, rightAngle) = leftCorn is Anchor.TopLeft or Anchor.BottomRight ? (propAngle1, propAngle2) : (propAngle2, propAngle1);

                var updLeftCorn = TransformHelper.TwoLineIntersection(leftCornPos + delta, rightCornPos + delta, center, center + Vector2d.FromAngleAndLength(leftAngle, 1));
                var updRightCorn = TransformHelper.TwoLineIntersection(leftCornPos + delta, rightCornPos + delta, center, center + Vector2d.FromAngleAndLength(rightAngle, 1));
                var updLeftOppCorn = TransformHelper.TwoLineIntersection(leftOppCornPos, rightOppCornPos, center, center + Vector2d.FromAngleAndLength(rightAngle, 1));
                var updRightOppCorn = TransformHelper.TwoLineIntersection(leftOppCornPos, rightOppCornPos, center, center + Vector2d.FromAngleAndLength(leftAngle, 1));

                if (updLeftCorn is null || updRightCorn is null || updLeftOppCorn is null || updRightOppCorn is null)
                    goto fallback;

                corners = TransformHelper.UpdateCorner(corners, leftCorn, (Vector2d)updLeftCorn);
                corners = TransformHelper.UpdateCorner(corners, rightCorn, (Vector2d)updRightCorn);
                corners = TransformHelper.UpdateCorner(corners, leftOppCorn, (Vector2d)updLeftOppCorn);
                corners = TransformHelper.UpdateCorner(corners, rightOppCorn, (Vector2d)updRightOppCorn);

                return corners;
            }
fallback:
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
            var (leftCorner, rightCorner) = TransformHelper.GetCornersOnSide(targetSide);
            var leftCornerPos = TransformHelper.GetAnchorPosition(corners, leftCorner);
            var rightCornerPos = TransformHelper.GetAnchorPosition(corners, rightCorner);
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetSide);

            var opposite = TransformHelper.GetOpposite(targetSide);
            var oppPos = TransformHelper.GetAnchorPosition(corners, opposite);

            if (freedom == TransformSideFreedom.Shear)
                desiredPos = desiredPos.ProjectOntoLine(leftCornerPos, rightCornerPos);
            else if (freedom == TransformSideFreedom.Stretch)
                desiredPos = desiredPos.ProjectOntoLine(targetPos, oppPos);

            var delta = desiredPos - targetPos;
            var newCorners = TransformHelper.UpdateCorner(corners, leftCorner, leftCornerPos + delta);
            newCorners = TransformHelper.UpdateCorner(newCorners, rightCorner, rightCornerPos + delta);

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
