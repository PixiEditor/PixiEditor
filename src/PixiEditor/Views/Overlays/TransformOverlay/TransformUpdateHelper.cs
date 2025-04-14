using Avalonia;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.Views.Overlays.TransformOverlay;
#nullable enable
internal static class TransformUpdateHelper
{
    private const double epsilon = 0.00001;

    public static ShapeCorners? UpdateShapeFromCorner
    (Anchor targetCorner, TransformCornerFreedom freedom, double propAngle1, double propAngle2, ShapeCorners corners,
        VecD desiredPos, bool scaleFromCenter,
        SnappingController? snappingController, out string snapX, out string snapY)
    {
        if (!TransformHelper.IsCorner(targetCorner))
            throw new ArgumentException($"{targetCorner} is not a corner");

        if (freedom == TransformCornerFreedom.Locked)
        {
            snapX = snapY = "";
            return corners;
        }

        if (freedom is TransformCornerFreedom.ScaleProportionally or TransformCornerFreedom.Scale)
        {
            // find opposite corners
            VecD targetPos = TransformHelper.GetAnchorPosition(corners, targetCorner);
            Anchor opposite = TransformHelper.GetOpposite(targetCorner);
            VecD oppositePos = TransformHelper.GetAnchorPosition(corners, opposite);

            snapX = snapY = "";

            // constrain desired pos to a "proportional" diagonal line if needed
            if (freedom == TransformCornerFreedom.ScaleProportionally && corners.IsRect)
            {
                double correctAngle = targetCorner is Anchor.TopLeft or Anchor.BottomRight ? propAngle1 : propAngle2;
                VecD direction = VecD.FromAngleAndLength(correctAngle, 1);
                desiredPos = desiredPos.ProjectOntoLine(oppositePos, oppositePos + direction);

                if (snappingController is not null)
                {
                    desiredPos = snappingController.GetSnapPoint(desiredPos, direction, out snapX, out snapY);
                }
            }
            else if (freedom == TransformCornerFreedom.ScaleProportionally)
            {
                desiredPos = desiredPos.ProjectOntoLine(oppositePos, targetPos);
                VecD direction = (targetPos - oppositePos);

                if (snappingController is not null)
                {
                    desiredPos = snappingController.GetSnapPoint(desiredPos, direction, out snapX, out snapY);
                }
            }
            else
            {
                if (snappingController is not null)
                {
                    desiredPos = snappingController.GetSnapPoint(desiredPos, out snapX, out snapY);
                }
            }

            // find neighboring corners
            (Anchor leftNeighbor, Anchor rightNeighbor) = TransformHelper.GetNeighboringCorners(targetCorner);
            VecD leftNeighborPos = TransformHelper.GetAnchorPosition(corners, leftNeighbor);
            VecD rightNeighborPos = TransformHelper.GetAnchorPosition(corners, rightNeighbor);

            // find positions of neighboring corners relative to the opposite corner
            VecD targetTrans = targetPos - oppositePos;
            VecD leftNeighTrans = leftNeighborPos - oppositePos;
            VecD rightNeighTrans = rightNeighborPos - oppositePos;

            // find by how much move each corner
            VecD delta = desiredPos - targetPos;
            VecD leftNeighDelta, rightNeighDelta;
            VecD oppositeDelta = scaleFromCenter ? -delta : VecD.Zero;

            if (corners.IsPartiallyDegenerate)
            {
                // handle cases where we'd need to scale by infinity
                leftNeighDelta = TransferZeros(leftNeighTrans, delta);
                rightNeighDelta = TransferZeros(rightNeighTrans, delta);
            }
            else
            {
                VecD? newLeftPos = TransformHelper.TwoLineIntersection(
                    oppositeDelta, leftNeighTrans + oppositeDelta, 
                    targetTrans + delta, leftNeighTrans + delta);
                VecD? newRightPos = TransformHelper.TwoLineIntersection(
                    oppositeDelta, rightNeighTrans + oppositeDelta,
                    targetTrans + delta, rightNeighTrans + delta);
                if (newLeftPos is null || newRightPos is null)
                    return null;
                leftNeighDelta = newLeftPos.Value - leftNeighTrans;
                rightNeighDelta = newRightPos.Value - rightNeighTrans;
            }

            // handle cases where the transform overlay is squished into a line or a single point
            bool squishedWithLeft = leftNeighTrans.TaxicabLength < epsilon;
            bool squishedWithRight = rightNeighTrans.TaxicabLength < epsilon;
            if (squishedWithLeft && squishedWithRight)
            {
                leftNeighDelta = TransferZeros(new(0, 1), delta);
                rightNeighDelta = TransferZeros(new(1, 0), delta);
            }
            else if (squishedWithLeft)
            {
                leftNeighDelta = TransferZeros(SwapAxes(rightNeighTrans), delta);
            }
            else if (squishedWithRight)
            {
                rightNeighDelta = TransferZeros(SwapAxes(leftNeighTrans), delta);
            }

            // move the corners, finally
            corners = TransformHelper.UpdateCorner(corners, targetCorner,
                targetTrans + delta + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, opposite,
                oppositePos + oppositeDelta);
            corners = TransformHelper.UpdateCorner(corners, leftNeighbor,
                leftNeighTrans + leftNeighDelta + oppositePos);
            corners = TransformHelper.UpdateCorner(corners, rightNeighbor,
                rightNeighTrans + rightNeighDelta + oppositePos);
            
            if (!corners.IsLegal)
                return null;

            return corners;
        }

        if (freedom == TransformCornerFreedom.Free)
        {
            snapX = snapY = "";
            ShapeCorners newCorners = TransformHelper.UpdateCorner(corners, targetCorner, desiredPos);
            return newCorners.IsLegal ? newCorners : null;
        }

        throw new ArgumentException($"Freedom degree {freedom} is not supported");
    }

    private static VecD SwapAxes(VecD vec) => new VecD(vec.Y, vec.X);

    private static VecD TransferZeros(VecD from, VecD to)
    {
        if (Math.Abs(from.X) < epsilon)
            to.X = 0;
        if (Math.Abs(from.Y) < epsilon)
            to.Y = 0;
        return to;
    }

    public static ShapeCorners? UpdateShapeFromSide
    (Anchor targetSide, TransformSideFreedom freedom, double propAngle1, double propAngle2, ShapeCorners corners,
        VecD desiredPos, bool scaleFromCenter, SnappingController? snappingController, out string snapX,
        out string snapY)
    {
        if (!TransformHelper.IsSide(targetSide))
            throw new ArgumentException($"{targetSide} is not a side");

        snapX = snapY = "";

        if (freedom == TransformSideFreedom.Locked)
            return corners;

        if (freedom is TransformSideFreedom.ScaleProportionally)
        {
            var targetPos = TransformHelper.GetAnchorPosition(corners, targetSide);
            var opposite = TransformHelper.GetOpposite(targetSide);
            var oppositePos = TransformHelper.GetAnchorPosition(corners, opposite);

            desiredPos = desiredPos.ProjectOntoLine(targetPos, oppositePos);

            VecD direction = targetPos - oppositePos;
            direction = VecD.FromAngleAndLength(direction.Angle, 1 / direction.Length);

            if (snappingController is not null)
            {
                VecD scaleDirection = corners.RectCenter - oppositePos;
                desiredPos = snappingController.GetSnapPoint(desiredPos, scaleDirection, out snapX, out snapY);
            }

            double scalingFactor = (desiredPos - oppositePos) * direction;
            if (!double.IsNormal(scalingFactor))
                return corners;

            if (corners.IsRect)
            {
                var delta = (desiredPos - targetPos);
                var center = oppositePos.Lerp(desiredPos, 0.5);

                if (scaleFromCenter)
                {
                    return ScaleEvenlyToPos(corners, desiredPos, targetPos);
                }

                var (leftCorn, rightCorn) = TransformHelper.GetCornersOnSide(targetSide);
                var (leftOppCorn, _) = TransformHelper.GetNeighboringCorners(leftCorn);
                var (_, rightOppCorn) = TransformHelper.GetNeighboringCorners(rightCorn);

                var leftCornPos = TransformHelper.GetAnchorPosition(corners, leftCorn);
                var rightCornPos = TransformHelper.GetAnchorPosition(corners, rightCorn);
                var leftOppCornPos = TransformHelper.GetAnchorPosition(corners, leftOppCorn);
                var rightOppCornPos = TransformHelper.GetAnchorPosition(corners, rightOppCorn);

                var (leftAngle, rightAngle) = leftCorn is Anchor.TopLeft or Anchor.BottomRight
                    ? (propAngle1, propAngle2)
                    : (propAngle2, propAngle1);

                var updLeftCorn = TransformHelper.TwoLineIntersection(leftCornPos + delta, rightCornPos + delta, center,
                    center + VecD.FromAngleAndLength(leftAngle, 1));
                var updRightCorn = TransformHelper.TwoLineIntersection(leftCornPos + delta, rightCornPos + delta,
                    center, center + VecD.FromAngleAndLength(rightAngle, 1));

                var updLeftOppCorn = TransformHelper.TwoLineIntersection(leftOppCornPos, rightOppCornPos, center,
                    center + VecD.FromAngleAndLength(rightAngle, 1));
                var updRightOppCorn = TransformHelper.TwoLineIntersection(leftOppCornPos, rightOppCornPos, center,
                    center + VecD.FromAngleAndLength(leftAngle, 1));

                if (updLeftCorn is null || updRightCorn is null || updLeftOppCorn is null || updRightOppCorn is null)
                    goto fallback;

                corners = TransformHelper.UpdateCorner(corners, leftCorn, (VecD)updLeftCorn);
                corners = TransformHelper.UpdateCorner(corners, rightCorn, (VecD)updRightCorn);
                corners = TransformHelper.UpdateCorner(corners, leftOppCorn, (VecD)updLeftOppCorn);
                corners = TransformHelper.UpdateCorner(corners, rightOppCorn, (VecD)updRightOppCorn);

                return corners;
            }

            fallback:

            if (scaleFromCenter)
            {
                return ScaleEvenlyToPos(corners, desiredPos, targetPos);
            }

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
            {
                desiredPos = desiredPos.ProjectOntoLine(leftCornerPos, rightCornerPos);
                if (snappingController is not null)
                {
                    VecD direction = corners.RectCenter - desiredPos;
                    desiredPos = snappingController.GetSnapPoint(desiredPos, direction, out snapX, out snapY);
                }
            }
            else if (freedom == TransformSideFreedom.Stretch)
            {
                if ((targetPos - oppPos).TaxicabLength > epsilon)
                    desiredPos = desiredPos.ProjectOntoLine(targetPos, oppPos);
                else
                    desiredPos = desiredPos.ProjectOntoLine(targetPos,
                        (leftCornerPos - targetPos).Rotate(Math.PI / 2) + targetPos);

                if (snappingController is not null)
                {
                    VecD direction = desiredPos - targetPos;
                    desiredPos = snappingController.GetSnapPoint(desiredPos, direction, out snapX, out snapY);
                }
            }

            var delta = desiredPos - targetPos;

            var newCorners = TransformHelper.UpdateCorner(corners, leftCorner, leftCornerPos + delta);
            newCorners = TransformHelper.UpdateCorner(newCorners, rightCorner, rightCornerPos + delta);

            if (scaleFromCenter)
            {
                VecD oppositeDelta = -delta;
                Anchor leftCornerOpp = TransformHelper.GetOpposite(leftCorner);
                Anchor rightCornerOpp = TransformHelper.GetOpposite(rightCorner);

                var leftCornerOppPos = TransformHelper.GetAnchorPosition(corners, leftCornerOpp);
                var rightCornerOppPos = TransformHelper.GetAnchorPosition(corners, rightCornerOpp);

                newCorners = TransformHelper.UpdateCorner(newCorners, leftCornerOpp, leftCornerOppPos + oppositeDelta);
                newCorners =
                    TransformHelper.UpdateCorner(newCorners, rightCornerOpp, rightCornerOppPos + oppositeDelta);
            }

            return newCorners.IsLegal ? newCorners : null;
        }

        throw new ArgumentException($"Freedom degree {freedom} is not supported");
    }

    private static ShapeCorners ScaleEvenlyToPos(ShapeCorners corners, VecD desiredPos, VecD targetPos)
    {
        VecD currentCenter = corners.RectCenter;
        float targetPosToCenter = (float)(targetPos - currentCenter).Length;

        if (targetPosToCenter < epsilon)
            return corners;

        VecD reflectedDesiredPos = desiredPos.ReflectAcrossLine(currentCenter, targetPos);

        float desiredPosToCenter = (float)(reflectedDesiredPos - currentCenter).Length;

        float scaling = desiredPosToCenter / targetPosToCenter;

        corners = corners.AsScaled(scaling);
        return corners;
    }

    public static ShapeCorners UpdateShapeFromRotation(ShapeCorners corners, VecD origin, double angle)
    {
        corners.TopLeft = corners.TopLeft.Rotate(angle, origin);
        corners.TopRight = corners.TopRight.Rotate(angle, origin);
        corners.BottomLeft = corners.BottomLeft.Rotate(angle, origin);
        corners.BottomRight = corners.BottomRight.Rotate(angle, origin);
        return corners;
    }
}
