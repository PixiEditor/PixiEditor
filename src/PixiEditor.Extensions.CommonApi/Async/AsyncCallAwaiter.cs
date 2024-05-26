using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.CommonApi.Async;

public class AsyncCallAwaiter : INotifyCompletion
{
    private readonly AsyncCall _task;
    
    public AsyncCallAwaiter(AsyncCall task)
    {
        _task = task;
    }
    
    public bool IsCompleted => _task.IsCompleted;
    
    public void OnCompleted(Action continuation) => _task.RegisterContinuation(continuation);
}

public class AsyncCallAwaiter<T> : INotifyCompletion
{
    private readonly AsyncCall<T> _task;
    
    public AsyncCallAwaiter(AsyncCall<T> task)
    {
        _task = task;
    }
    
    public bool IsCompleted => _task.IsCompleted;
    
    public void OnCompleted(Action continuation) => _task.RegisterContinuation(continuation);
    
    public T GetResult() => _task.Result;
}
