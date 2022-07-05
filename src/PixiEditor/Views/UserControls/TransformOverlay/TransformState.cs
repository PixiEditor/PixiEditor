using ChunkyImageLib.DataHolders;

namespace PixiEditor.Views.UserControls.TransformOverlay;
internal struct TransformState
{
    public bool OriginWasManuallyDragged { get; set; }
    public VecD Origin { get; set; }
    public double ProportionalAngle1 { get; set; }
    public double ProportionalAngle2 { get; set; }
}
