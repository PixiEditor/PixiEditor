using Avalonia.Animation;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.UI.Common.Animators;

namespace PixiEditor.UI.Common.Themes;

public class PixiEditorTheme : Styles
{
    public PixiEditorTheme(IServiceProvider? sp = null)
    {
        Animation.RegisterCustomAnimator<Geometry, MorphAnimator>();
        AvaloniaXamlLoader.Load(sp, this);
    }
}
