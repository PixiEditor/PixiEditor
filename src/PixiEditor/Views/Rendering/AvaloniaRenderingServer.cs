using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Skia.Extensions;
using PixiEditor.Views.Visuals;

namespace PixiEditor.Views.Rendering;

public class AvaloniaRenderingServer : IRenderingServer
{
    public Action<Action> Invoke { get; } = action => Dispatcher.UIThread.Invoke(action);
}
