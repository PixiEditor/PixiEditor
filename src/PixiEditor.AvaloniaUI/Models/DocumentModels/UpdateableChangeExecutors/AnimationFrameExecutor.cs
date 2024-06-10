using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.ChangeInfos.Animation;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;

internal class AnimationFrameExecutor : UpdateableChangeExecutor
{
    private readonly int activeFrame;
    
    public AnimationFrameExecutor(int activeFrame)
    {
        this.activeFrame = activeFrame;
    }
    
    public override ExecutionState Start()
    {
        internals.ActionAccumulator.AddActions(new ActiveFrame_Action(activeFrame));
        
        return ExecutionState.Success;
    }

    public override void ActiveFrameChanged(int newActiveFrame)
    {
        if (newActiveFrame == activeFrame)
            return;
        internals.ActionAccumulator.AddActions(new ActiveFrame_Action(newActiveFrame));
    }

    public override void OnActiveAnimationFrameEnded()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndActiveFrame_Action());
        onEnded?.Invoke(this);
    }

    public override void ForceStop()
    {
        internals.ActionAccumulator.AddFinishedActions(new EndActiveFrame_Action());
    }
}
