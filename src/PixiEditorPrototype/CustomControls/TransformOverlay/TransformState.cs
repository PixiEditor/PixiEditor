using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.CustomControls.TransformOverlay;
internal struct TransformState
{
    public bool OriginWasManuallyDragged { get; set; }
    public Vector2d Origin { get; set; }
    public double ProportionalAngle1 { get; set; }
    public double ProportionalAngle2 { get; set; }
}
