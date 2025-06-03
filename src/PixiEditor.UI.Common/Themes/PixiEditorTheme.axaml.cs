using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace PixiEditor.UI.Common.Themes;

public class PixiEditorTheme : Styles
{
    public PixiEditorTheme(IServiceProvider? sp = null)
    {
        AvaloniaXamlLoader.Load(sp, this);
        if (OperatingSystem.IsMacOS())
        {
            Application.Current.Styles.Resources["ContentControlThemeFontFamily"] = FontFamily.Parse("Arial");
        }
    }
}
