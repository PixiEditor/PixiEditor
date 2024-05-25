namespace PixiEditor.Extensions.CommonApi.Async;

public delegate void AsyncCallCompleted();
public delegate void AsyncCallCompleted<T>(T result);
public delegate void AsyncCallFailed(Exception exception);
    
public class AsyncCall
{
    private object? _result;
    public AsyncCallState State { get; protected set; } = AsyncCallState.Pending;
    public bool IsCompleted => State != AsyncCallState.Pending;
    public Exception? Exception { get; protected set; }

    public object Result
    {
        get
        {
            return _result;
        }
        protected set
        {
            _result = SetResultValue(value);
        }
    }
    
    public event AsyncCallCompleted Completed;
    public event AsyncCallFailed Failed;
    
    public void SetException(Exception exception)
    {
        if (State != AsyncCallState.Pending)
        {
            throw new InvalidOperationException("Cannot set exception on completed async call.");
        }
        
        State = AsyncCallState.Failed;
        Exception = exception;
        Failed?.Invoke(exception);
    }
    
    public void SetResult(object? result)
    {
        if (State != AsyncCallState.Pending)
        {
            throw new InvalidOperationException("Cannot set result on completed async call.");
        }
        
        State = AsyncCallState.Completed;
        Result = result;
        Completed?.Invoke();
    }
    
    protected virtual object SetResultValue(object? result)
    {
        return result;
    }
}

public class AsyncCall<TResult> : AsyncCall
{
    public new TResult Result
    {
        get => (TResult) base.Result;
        protected set => base.Result = value;
    }
    
    public new event AsyncCallCompleted<TResult> Completed;
    
    public AsyncCall()
    {
        base.Completed += () => Completed?.Invoke(Result);
    }
    
    public Task<TResult> AsTask()
    {
        TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
        Completed += (result) => tcs.SetResult(result);
        Failed += (exception) => tcs.SetException(exception);
        return tcs.Task;
    }
    
    public void SetResult(TResult result)
    {
        if (State != AsyncCallState.Pending)
        {
            throw new InvalidOperationException("Cannot set result on completed async call.");
        }
        
        State = AsyncCallState.Completed;
        Result = result;
        Completed?.Invoke(result);
    }
    
    public AsyncCall<T> ContinueWith<T>(Func<AsyncCall<TResult>, T> action)
    {
        AsyncCall<T> asyncCall = new AsyncCall<T>();
        Completed += (result) => asyncCall.SetResult(action(this));
        Failed += (exception) => asyncCall.SetException(exception);
        return asyncCall;
    }
    
    public AsyncCall ContinueWith(Action<AsyncCall<TResult>> action)
    {
        AsyncCall asyncCall = new AsyncCall();
        Completed += (result) =>
        {
            action(this);
            asyncCall.SetResult(null);
        };
        Failed += (exception) => asyncCall.SetException(exception);
        return asyncCall;
    }

    public static AsyncCall<TResult> FromTask(Task<TResult> task)
    {
        AsyncCall<TResult> asyncCall = new AsyncCall<TResult>();
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                asyncCall.SetException(t.Exception);
            }
            else
            {
                asyncCall.SetResult(t.Result);
            }
        });
        return asyncCall;
    }
}

public enum AsyncCallState
{
    Pending,
    Completed,
    Failed
}
