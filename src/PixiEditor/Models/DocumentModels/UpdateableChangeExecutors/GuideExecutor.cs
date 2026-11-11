using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.Tools;
using PixiEditor.Views.Overlays.Ruler;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class GuideExecutor : UpdateableChangeExecutor
{
    private readonly Guid guideId;

    public GuideExecutor(Guid id)
    {
        guideId = id;
    }

    public override ExecutionState Start()
    {
        var guide = document.GuidesHandler.GuidesHandler.FirstOrDefault(x => x.Id == guideId);

        if (guide == null)
            return ExecutionState.Error;

        double lastPos = guide.Offset;

        internals.ActionAccumulator.AddActions(new GuidePosition_Action(guideId, lastPos));

        return ExecutionState.Success;
    }

    public override void OnGuideDragged(GuideDragInfo info)
    {
        if (info.Id != guideId)
            return;

        internals.ActionAccumulator.AddActions(new GuidePosition_Action(guideId, info.NewPosition));
    }

    public override void OnGuideDragEnded(Guid id)
    {
        internals.ActionAccumulator.AddFinishedActions(new EndGuidePosition_Action());
        onEnded!(this);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndGuidePosition_Action());
    }
}
