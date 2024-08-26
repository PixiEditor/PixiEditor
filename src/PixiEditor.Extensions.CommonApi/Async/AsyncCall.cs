using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.CommonApi.Async;

public delegate void AsyncCallCompleted();
public delegate void AsyncCallCompleted<T>(T result);
public delegate void AsyncCallFailed(Exception exception);
    
[AsyncMethodBuilder(typeof(AsyncCallAsyncMethodBuilder))]
public class AsyncCall
{
    private List<AsyncCallCompleted> completedCalls = new List<AsyncCallCompleted>();
    private List<AsyncCallFailed> failedCalls = new List<AsyncCallFailed>();
    private object? _result;
    protected Action continuation;
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
    
    public event AsyncCallCompleted Completed
    {
        add
        {
            if (State == AsyncCallState.Completed)
            {
                value();
            }
            else
            {
                completedCalls.Add(value);
            }
        }
        remove
        {
            completedCalls.Remove(value);
        }
    }
    public event AsyncCallFailed Failed
    {
        add
        {
            if (State == AsyncCallState.Failed)
            {
                value(Exception);
            }
            else
            {
                failedCalls.Add(value);
            }
        }
        remove
        {
            failedCalls.Remove(value);
        }
    }
    
    public void SetException(Exception exception)
    {
        if (State != AsyncCallState.Pending)
        {
            throw new InvalidOperationException("Cannot set exception on completed async call.");
        }
        
        State = AsyncCallState.Failed;
        Exception = exception;
        this.continuation?.Invoke();

        InvokeFailed(exception);
    }

    public void SetResult(object? result)
    {
        if (State != AsyncCallState.Pending)
        {
            throw new InvalidOperationException("Cannot set result on completed async call.");
        }
        
        State = AsyncCallState.Completed;
        Result = result;
        this.continuation?.Invoke();
        InvokeCompleted();
    }
    
    public AsyncCallAwaiter GetAwaiter()
    {
        return new AsyncCallAwaiter(this);
    }
    
    protected virtual object SetResultValue(object? result)
    {
        return result;
    }

    private void InvokeCompleted()
    {
        foreach (var completedCall in completedCalls)
        {
            completedCall();
        }
    }
    
    private void InvokeFailed(Exception exception)
    {
        foreach (var failedCall in failedCalls)
        {
            failedCall(exception);
        }
    }
    
    internal void RegisterContinuation(Action cont)
    {
        if (State == AsyncCallState.Pending)
        {
            if (this.continuation is null)
            {
                this.continuation = cont;  
            }
            else
            {
                var prev = this.continuation;
                this.continuation = () =>
                {
                    prev();
                    cont();
                };
            }
        }
        else
        {
            cont();
        }
    }
}

[AsyncMethodBuilder(typeof(AsyncCallAsyncMethodBuilder<>))]
public class AsyncCall<TResult> : AsyncCall
{
    private List<AsyncCallCompleted<TResult>> completedCalls = new List<AsyncCallCompleted<TResult>>();
    public new TResult Result
    {
        get => (TResult) base.Result;
        protected set => base.Result = value;
    }
    
    public new event AsyncCallCompleted<TResult> Completed
    {
        add
        {
            if (State == AsyncCallState.Completed)
            {
                value(Result);
            }
            else
            {
                completedCalls.Add(value);
            }
        }
        remove
        {
            completedCalls.Remove(value);
        }
    }
    
    public AsyncCall()
    {
        base.Completed += () =>
        {
            InvokeCompleted(Result);
        };
    }
    
    public AsyncCallAwaiter<TResult> GetAwaiter()
    {
        return new AsyncCallAwaiter<TResult>(this);
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
        continuation?.Invoke();
        InvokeCompleted(result);
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
    
    public static AsyncCall<TResult> FromResult(TResult result)
    {
        AsyncCall<TResult> asyncCall = new AsyncCall<TResult>();
        asyncCall.SetResult(result);
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
    
    private void InvokeCompleted(TResult result)
    {
        foreach (var completedCall in completedCalls)
        {
            completedCall(result);
        }
    }
}

public enum AsyncCallState
{
    Pending,
    Completed,
    Failed
}
