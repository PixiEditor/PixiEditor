using Avalonia;

namespace PixiEditor.Extensions.UI;

public interface ICustomTranslatorElement
{
    public void SetTranslationBinding(AvaloniaProperty dependencyProperty, IObservable<string> binding);
    public AvaloniaProperty GetDependencyProperty();
}
