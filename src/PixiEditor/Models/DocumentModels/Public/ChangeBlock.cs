namespace PixiEditor.Models.DocumentModels.Public;

public class ChangeBlock : IDisposable
{
    private ActionAccumulator Accumulator { get; }
    
    internal ChangeBlock(ActionAccumulator accumulator)
    {
        Accumulator = accumulator;
        Accumulator.StartChangeBlock();
    }
    
    public async Task ExecuteQueuedActions()
    {
        await Accumulator.TryExecuteAccumulatedActions();
    }
    
    public void Dispose()
    {
        Accumulator.EndChangeBlock();
    }
}
