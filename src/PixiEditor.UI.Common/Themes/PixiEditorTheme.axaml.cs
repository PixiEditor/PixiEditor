using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace PixiEditor.UI.Common.Themes;

public class PixiEditorTheme : Styles
{
    public PixiEditorTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
    }
}
