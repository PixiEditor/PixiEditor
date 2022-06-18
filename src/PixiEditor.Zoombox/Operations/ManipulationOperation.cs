using System;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Zoombox.Operations;

internal class ManipulationOperation
{
    private readonly Zoombox owner;
    private LockingRotationProcess? rotationProcess;

    private double updatedAngle;
    private double initialAngle;
    private bool startedRotating = false;

    public ManipulationOperation(Zoombox owner)
    {
        this.owner = owner;
    }

    public void Start()
    {
        updatedAngle = owner.Angle;
        initialAngle = owner.Angle;
        rotationProcess = new LockingRotationProcess(owner.Angle);
    }

    public void Update(ManipulationDeltaEventArgs args)
    {
        args.Handled = true;
        VecD screenTranslation = new(args.DeltaManipulation.Translation.X, args.DeltaManipulation.Translation.Y);
        VecD screenOrigin = new(args.ManipulationOrigin.X, args.ManipulationOrigin.Y);
        double deltaAngle = args.DeltaManipulation.Rotation / 180 * Math.PI;
        if (owner.FlipX ^ owner.FlipY)
            deltaAngle = -deltaAngle;
        Manipulate(args.DeltaManipulation.Scale.X, screenTranslation, screenOrigin, deltaAngle);
    }

    private void Manipulate(double deltaScale, VecD screenTranslation, VecD screenOrigin, double rotation)
    {
        double newScale = Math.Clamp(owner.Scale * deltaScale, owner.MinScale, Zoombox.MaxScale);

        updatedAngle += rotation;
        if (!startedRotating && Math.Abs(ZoomboxOperationHelper.SubtractOnCircle(initialAngle, updatedAngle)) > 0.35)
            startedRotating = true;

        double newAngle = startedRotating ? rotationProcess!.UpdateRotation(updatedAngle) : initialAngle;

        VecD originalPos = owner.ToZoomboxSpace(screenOrigin);
        owner.Angle = newAngle;
        owner.Scale = newScale;
        VecD newPos = owner.ToZoomboxSpace(screenOrigin);
        VecD centerTranslation = originalPos - newPos;
        owner.Center += centerTranslation;

        VecD translatedZoomboxPos = owner.ToZoomboxSpace(screenOrigin + screenTranslation);
        owner.Center -= translatedZoomboxPos - originalPos;
    }
}
