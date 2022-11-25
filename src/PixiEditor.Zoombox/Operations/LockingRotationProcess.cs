using System;

namespace PixiEditor.Zoombox.Operations;

internal class LockingRotationProcess
{
    private double currentAngleWithoutLock;

    private bool isLocked = false;
    private double angleThatWeLockedTo = 0;
    private bool angleWasIncreasingDuringLock = false;

    public LockingRotationProcess(double initialAngle)
    {
        this.currentAngleWithoutLock = initialAngle;
    }

    /// <returns>New rotation angle with locking taken into account</returns>
    public double UpdateRotation(double newAngle)
    {
        return isLocked ? UpdateLockedRotation(newAngle) : UpdateUnlockedRotation(newAngle);
    }

    private double UpdateUnlockedRotation(double newAngle)
    {
        double? lockingAngle = FindAngleToLockOn(currentAngleWithoutLock, newAngle);
        if (lockingAngle is not null)
        {
            isLocked = true;
            angleThatWeLockedTo = lockingAngle.Value;
            angleWasIncreasingDuringLock = ZoomboxOperationHelper.SubtractOnCircle(newAngle, currentAngleWithoutLock) > 0;
            currentAngleWithoutLock = newAngle;
            return angleThatWeLockedTo;
        }

        currentAngleWithoutLock = newAngle;
        return currentAngleWithoutLock;
    }

    private double UpdateLockedRotation(double newAngle)
    {
        currentAngleWithoutLock = newAngle;
        double deviationFromLocked = ZoomboxOperationHelper.SubtractOnCircle(newAngle, angleThatWeLockedTo);

        if (Math.Abs(deviationFromLocked) > 0.35 ||
            angleWasIncreasingDuringLock ^ (deviationFromLocked > 0))
        {
            isLocked = false;
            return currentAngleWithoutLock;
        }
        return angleThatWeLockedTo;
    }

    private static bool IsWithin(double point, double rangeStart, double rangeEnd)
    {
        double startDist = ZoomboxOperationHelper.SubtractOnCircle(rangeStart, point);
        double endDist = ZoomboxOperationHelper.SubtractOnCircle(point, rangeEnd);
        return startDist != 0 && endDist != 0 && Math.Sign(startDist) == Math.Sign(endDist) && Math.Abs(startDist) + Math.Abs(endDist) < Math.PI;
    }

    private static double? FindAngleToLockOn(double prevAngle, double newAngle)
    {
        prevAngle = ZoomboxOperationHelper.Mod(prevAngle, Math.PI * 2);
        newAngle = ZoomboxOperationHelper.Mod(newAngle, Math.PI * 2);

        if (IsWithin(0, prevAngle, newAngle))
            return 0;
        if (IsWithin(Math.PI / 2, prevAngle, newAngle))
            return Math.PI / 2;
        if (IsWithin(Math.PI, prevAngle, newAngle))
            return Math.PI;
        if (IsWithin(Math.PI * 3 / 2, prevAngle, newAngle))
            return Math.PI * 3 / 2;

        return null;
    }
}
