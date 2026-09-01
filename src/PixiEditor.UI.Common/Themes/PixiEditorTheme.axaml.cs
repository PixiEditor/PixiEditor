using Avalonia;
using Avalonia.Controls.Chrome;
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

        AddWindowDecorations();
    }

    private void AddWindowDecorations()
    {
        if (OperatingSystem.IsLinux() && TryGetResource("LinuxWindowDecorations", null, out var decorTheme) && decorTheme is ControlTheme linuxControlTheme)
        {
            Application.Current.Styles.Resources.Add(typeof(WindowDrawnDecorations), linuxControlTheme);
        }

        if (OperatingSystem.IsWindows() && TryGetResource("WindowsWindowDecorations", null, out var windecorTheme) &&
            windecorTheme is ControlTheme winControlTheme)
        {
            Application.Current.Styles.Resources.Add(typeof(WindowDrawnDecorations), winControlTheme);
        }
    }
}
