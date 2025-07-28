using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.WasmRuntime.Management;

internal delegate void AsyncCallCompleted(int asyncHandle, int resultValue); 
internal delegate void AsyncCallFaulted(int asyncHandle, string exceptionMessage); 
internal class AsyncCallsManager 
{
    private Dictionary<int, AsyncCall> asyncCalls = new();
 
    public event AsyncCallCompleted OnAsyncCallCompleted;
    public event AsyncCallFaulted OnAsyncCallFaulted;
    
    public int AddAsyncCall(AsyncCall<int> task)
    {
        int asyncHandle = GetNextAsyncHandle();
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                OnAsyncCallFaulted?.Invoke(asyncHandle, t.Exception.Message);
            }
            else
            {
                OnAsyncCallCompleted?.Invoke(asyncHandle, t.Result);
            }

            RemoveAsyncCall(asyncHandle);
        });
        asyncCalls[asyncHandle] = task;
        
        return asyncHandle;
    }
    
    public void RemoveAsyncCall(int asyncHandle)
    {
        asyncCalls.Remove(asyncHandle);
    }
    
    public async AsyncCall<T> InvokeAsync<T>(Action<int> invokeAction)
    {
        int asyncHandle = GetNextAsyncHandle();
        AsyncCall<T> task = new();
        asyncCalls[asyncHandle] = task;
        invokeAction(asyncHandle);
        return await task;
    }
    
    public void SetAsyncCallResult<T>(int asyncHandle, T result)
    {
        if (asyncCalls.TryGetValue(asyncHandle, out AsyncCall asyncCall))
        {
            asyncCall.SetResult(result);
        }
    }

    private int GetNextAsyncHandle()
    {
        int asyncHandle = 0;
        
        while (asyncCalls.ContainsKey(asyncHandle))
        {
            asyncHandle++;
        }
        
        return asyncHandle;
    }
}
