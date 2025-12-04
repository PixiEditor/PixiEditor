using System;
using System.Linq;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

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
        updatedAngle = owner.AngleRadians;
        initialAngle = owner.AngleRadians;
        rotationProcess = new LockingRotationProcess(owner.AngleRadians);
    }

    //TODO: Implement this
    /*public void Update(ManipulationDeltaEventArgs args)
    {
        args.Handled = true;
        double thresholdFactor = 1;
        var manipulators = args.Manipulators.Select(man => Zoombox.ToVecD(man.GetPosition(owner.mainCanvas))).ToList();
        if (manipulators.Count >= 2)
        {
            double dist = (manipulators[0] - manipulators[1]).Length / 140;
            thresholdFactor = 1 / dist;
        }

        VecD screenTranslation = new(args.DeltaManipulation.Translation.X, args.DeltaManipulation.Translation.Y);
        VecD screenOrigin = new(args.ManipulationOrigin.X, args.ManipulationOrigin.Y);
        double deltaAngle = args.DeltaManipulation.Rotation / 180 * Math.PI;
        if (owner.FlipX ^ owner.FlipY)
            deltaAngle = -deltaAngle;
        Manipulate(args.DeltaManipulation.Scale.X, screenTranslation, screenOrigin, deltaAngle, thresholdFactor);
    }*/

    private void Manipulate(double deltaScale, VecD screenTranslation, VecD screenOrigin, double rotation, double thresholdFactor)
    {
        double newScale = Math.Clamp(owner.Scale * deltaScale, owner.MinScale, Zoombox.MaxScale);

        updatedAngle += rotation;
        if (!startedRotating && Math.Abs(ZoomboxOperationHelper.SubtractOnCircle(initialAngle, updatedAngle)) > 0.35 * thresholdFactor)
            startedRotating = true;

        double newAngle = startedRotating ? rotationProcess!.UpdateRotation(updatedAngle) : initialAngle;

        VecD originalPos = owner.ToZoomboxSpace(screenOrigin);
        owner.AngleRadians = newAngle;
        owner.Scale = newScale;
        VecD newPos = owner.ToZoomboxSpace(screenOrigin);
        VecD centerTranslation = originalPos - newPos;
        owner.Center += centerTranslation;
        //owner.Pan += centerTranslation;

        VecD translatedZoomboxPos = owner.ToZoomboxSpace(screenOrigin + screenTranslation);
        owner.Center -= translatedZoomboxPos - originalPos;
        owner.Pan -= translatedZoomboxPos - originalPos;
    }
}
