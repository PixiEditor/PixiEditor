using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public static class AsyncUtility
{
    public static AsyncCall<int> ToIntResultFrom<T>(AsyncCall<T> task)
    {
        return task.ContinueWith(t =>
        {
            if (t.Result is null)
            {
                return -1;
            }

            return Convert.ToInt32(t.Result);
        });
    }
}
