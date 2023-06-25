using System.Windows.Markup;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.AppExtensions.Services;

public class WindowProvider : IWindowProvider
{
    private Dictionary<string, Func<IPopupWindow>> _openHandlers = new();

    public WindowProvider RegisterHandler(string id, Func<IPopupWindow> handler)
    {
        if (_openHandlers.ContainsKey(id))
        {
            _openHandlers[id] = handler;
            throw new ArgumentException($"Window with id {id} already has a handler");
        }

        _openHandlers.Add(id, handler);
        return this;
    }

    public PopupWindow CreatePopupWindow(string title, object body)
    {
        return new PopupWindow(new BasicPopup { Title = title, Body = body });
    }

    public PopupWindow OpenWindow(WindowType type)
    {
        return OpenWindow($"PixiEditor.{type}");
    }

    public PopupWindow OpenWindow(string windowId)
    {
        var handler = _openHandlers.FirstOrDefault(x => x.Key == windowId);
        if (handler.Key != null)
        {
            return new PopupWindow(handler.Value());
        }
        else
        {
            throw new ArgumentException($"Window with id {windowId} does not exist");
        }
    }
}
