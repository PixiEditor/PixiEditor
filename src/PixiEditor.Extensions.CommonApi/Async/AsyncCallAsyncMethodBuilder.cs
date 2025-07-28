using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.CommonApi.Async;

public struct AsyncCallAsyncMethodBuilder
{
    private AsyncCall _task;
    
    public AsyncCall Task => _task;

    public static AsyncCallAsyncMethodBuilder Create()
    {
        return new AsyncCallAsyncMethodBuilder
        {
            _task = new AsyncCall()
        };
    }
    
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }
        
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        Action move = stateMachine.MoveNext;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            move();
        });
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void SetResult() => _task.SetResult(null);

    public void SetException(Exception exception) => _task.SetException(exception);
}

public struct AsyncCallAsyncMethodBuilder<T>
{
    private AsyncCall<T> _task;
    
    public AsyncCall<T> Task => _task;

    public static AsyncCallAsyncMethodBuilder<T> Create()
    {
        return new AsyncCallAsyncMethodBuilder<T>
        {
            _task = new AsyncCall<T>()
        };
    }
    
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }
        
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }

    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        Action move = stateMachine.MoveNext;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            move();
        });
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void SetResult(T result) => _task.SetResult(result);

    public void SetException(Exception exception) => _task.SetException(exception);
}
