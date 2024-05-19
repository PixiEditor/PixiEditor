namespace PixiEditor.Extensions.WasmRuntime.Utilities;

public static class AsyncUtility
{
    public static Task<int> ToResultFrom<T>(Task<T> task)
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
