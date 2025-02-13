namespace PixiEditor.Models.DocumentModels.Public;

public class ChangeBlock : IDisposable
{
    private ActionAccumulator Accumulator { get; }
    
    internal ChangeBlock(ActionAccumulator accumulator)
    {
        Accumulator = accumulator;
        Accumulator.StartChangeBlock();
    }
    
    public void ExecuteQueuedActions()
    {
        Accumulator.TryExecuteAccumulatedActions();
    }
    
    public void Dispose()
    {
        Accumulator.EndChangeBlock();
    }
}
