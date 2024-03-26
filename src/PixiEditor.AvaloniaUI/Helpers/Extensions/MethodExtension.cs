using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

public static class MethodExtension
{
    public static async Task<T> InvokeAsync<T>(this MethodInfo @this, object obj, params object[] parameters)
    {
        //TODO: uhh, make sure this is ok?
        Dispatcher.UIThread.InvokeAsync(async () => await Task.Run(async () =>
        {
            var task = (Task)@this.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return (T)resultProperty.GetValue(task);
        }));

        return default;
    }
}
