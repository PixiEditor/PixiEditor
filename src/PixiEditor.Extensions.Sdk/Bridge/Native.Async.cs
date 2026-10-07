using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.Sdk.Async;
using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal partial class Native
{
    private static Dictionary<int, AsyncCall> asyncCalls = new();
    
    [ApiExport("async_call_completed")]
    internal static void async_call_completed(int asyncHandle, IntPtr resultValue, int length)
    {
        if (!asyncCalls.ContainsKey(asyncHandle))
        {
            throw new InvalidOperationException($"Async call with handle {asyncHandle} does not exist.");
        }

        byte[] bytes = InteropUtility.IntPtrToByteArray(resultValue, length);

        asyncCalls[asyncHandle].SetResult(bytes);
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
    
    public static AsyncCall<T> CreateAsyncCall<T>(int asyncHandle)
    {
        AsyncCall<T> asyncCall = new();
        asyncCalls[asyncHandle] = asyncCall;

        return asyncCall;
    }
    
    public static AsyncCall<TResult> CreateAsyncCall<TResult>(int asyncHandle, Func<byte[], TResult> conversion)
    {
        ConvertableAsyncCall<TResult, byte[]> asyncCall = new(conversion);
        asyncCalls[asyncHandle] = asyncCall;

        return asyncCall;
    }
}
