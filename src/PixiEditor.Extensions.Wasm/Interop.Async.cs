using PixiEditor.Extensions.Wasm.Async;

namespace PixiEditor.Extensions.Wasm;

internal partial class Interop
{
    private static Dictionary<int, AsyncCall> asyncCalls = new();
    
    [ApiExport("async_call_completed")]
    internal static void async_call_completed(int asyncHandle, int resultValue)
    {
        if (!asyncCalls.ContainsKey(asyncHandle))
        {
            throw new InvalidOperationException($"Async call with handle {asyncHandle} does not exist.");
        }
        
        asyncCalls[asyncHandle].SetResult(resultValue);
        asyncCalls.Remove(asyncHandle);
    }
    
    [ApiExport("async_call_faulted")]
    internal static void async_call_faulted(int asyncHandle, string exceptionMessage)
    {
        if (!asyncCalls.ContainsKey(asyncHandle))
        {
            throw new InvalidOperationException($"Async call with handle {asyncHandle} does not exist.");
        }
        
        asyncCalls[asyncHandle].SetException(new InvalidOperationException(exceptionMessage));
        asyncCalls.Remove(asyncHandle);
    }
    
    // TODO: More types
    public static AsyncCall AsyncHandleToTask<T>(int asyncHandle)
    {
        AsyncCall asyncCall = new(asyncHandle);
        asyncCalls[asyncHandle] = asyncCall;

        return asyncCall;
    }
}
