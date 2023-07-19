using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;
using PixiEditor.Views.UserControls.SymmetryOverlay;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class SymmetryExecutor : UpdateableChangeExecutor
{
    private readonly SymmetryAxisDirection dir;

    public SymmetryExecutor(SymmetryAxisDirection dir)
    {
        this.dir = dir;
    }

    public override ExecutionState Start()
    {
        if (!document.HorizontalSymmetryAxisEnabledBindable && dir == SymmetryAxisDirection.Horizontal ||
            !document.VerticalSymmetryAxisEnabledBindable && dir == SymmetryAxisDirection.Vertical)
            return ExecutionState.Error;

        double lastPos = dir switch
        {
            SymmetryAxisDirection.Horizontal => document.HorizontalSymmetryAxisYBindable,
            SymmetryAxisDirection.Vertical => document.VerticalSymmetryAxisXBindable,
            _ => throw new NotImplementedException(),
        };
        internals.ActionAccumulator.AddActions(new SymmetryAxisPosition_Action(dir, lastPos));

        return ExecutionState.Success;
    }

    public override void OnSymmetryDragged(SymmetryAxisDragInfo info)
    {
        if (info.Direction != dir)
            return;
        internals.ActionAccumulator.AddActions(new SymmetryAxisPosition_Action(dir, info.NewPosition));
    }

    public override void OnSymmetryDragEnded(SymmetryAxisDirection dir)
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSymmetryAxisPosition_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndSymmetryAxisPosition_Action());
    }
}
