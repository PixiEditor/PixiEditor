using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.AvaloniaUI.Animators;

namespace PixiEditor.AvaloniaUI.Animation;

public class Animators : Styles
{
    public Animators(IServiceProvider? sp = null)
    {
        Avalonia.Animation.Animation.RegisterCustomAnimator<IDashStyle, SelectionDashAnimator>();
    }
}
