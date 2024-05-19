namespace PixiEditor.Extensions.WasmRuntime.Management;

internal delegate void AsyncCallCompleted(int asyncHandle, int resultValue); 
internal delegate void AsyncCallFaulted(int asyncHandle, string exceptionMessage); 
internal class AsyncCallsManager 
{
    private Dictionary<int, Task> asyncCalls = new();
 
    public event AsyncCallCompleted OnAsyncCallCompleted;
    public event AsyncCallFaulted OnAsyncCallFaulted;
    
    public int AddAsyncCall(Task<int> task)
    {
        int asyncHandle = GetNextAsyncHandle();
        task.ContinueWith(t =>
        {
            if (t.IsFaulted)
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
