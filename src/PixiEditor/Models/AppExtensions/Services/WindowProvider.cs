using System.Windows.Markup;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.AppExtensions.Services;

public class WindowProvider : IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, string bodyXaml)
    {
        object body = null;
        if(bodyXaml is not null)
        {
            body = XamlReader.Parse(bodyXaml);
        }

        return new PopupWindow(new BasicPopup { Title = title, Body = body });
    }
}
