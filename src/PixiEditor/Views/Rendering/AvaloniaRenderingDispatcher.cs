using Avalonia.Threading;
using Drawie.Backend.Core;

namespace PixiEditor.Views.Rendering;

public class AvaloniaRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => Dispatcher.UIThread.Invoke(action);
}
