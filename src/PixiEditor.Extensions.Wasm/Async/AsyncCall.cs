namespace PixiEditor.Extensions.Wasm.Async;

public class AsyncCall
{
    public int AsyncHandle { get; }
    private AsyncCallStatus AsyncState { get; set; } = AsyncCallStatus.NotStarted;
    public bool IsCompleted => AsyncState switch
    {
        AsyncCallStatus.Completed => true,
        AsyncCallStatus.Faulted => true,
        _ => false
    };
    private TaskCompletionSource<int> TaskCompletionSource { get; } = new TaskCompletionSource<int>();

    public event Action<int> Completed;
    public event Action<Exception> Faulted;
    
    public AsyncCall(int asyncHandle)
    {
        AsyncHandle = asyncHandle;
    }
    
    public void SetResult(int result)
    {
        TaskCompletionSource.SetResult(result);
        Completed?.Invoke(result);
    }

    public void SetException(Exception exception)
    {
        TaskCompletionSource.SetException(exception);
        Faulted?.Invoke(exception);
    }
}

public enum AsyncCallStatus
{
    NotStarted,
    Started,
    Running,
    Completed,
    Faulted
} 
