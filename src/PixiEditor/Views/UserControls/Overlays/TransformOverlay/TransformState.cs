﻿using System.CodeDom;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

#nullable enable
namespace PixiEditor.Views.UserControls.Overlays.TransformOverlay;
internal struct TransformState
{
    public bool OriginWasManuallyDragged { get; set; }
    public VecD Origin { get; set; }
    public double ProportionalAngle1 { get; set; }
    public double ProportionalAngle2 { get; set; }

    public bool AlmostEquals(TransformState other, double epsilon = 0.001)
    {
        return
            OriginWasManuallyDragged == other.OriginWasManuallyDragged &&
            other.Origin.AlmostEquals(Origin, epsilon) &&
            Math.Abs(ProportionalAngle1 - other.ProportionalAngle1) < epsilon &&
            Math.Abs(ProportionalAngle2 - other.ProportionalAngle2) < epsilon;
    }
}
