namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;

public interface IMidChangeUndoableExecutor : IExecutorFeature
{
    public void OnMidChangeUndo();
    public void OnMidChangeRedo();
    public bool CanUndo { get; }
    public bool CanRedo { get; }
}
