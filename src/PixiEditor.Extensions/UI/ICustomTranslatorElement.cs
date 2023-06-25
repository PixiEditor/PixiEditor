using Avalonia;
using Avalonia.Data;

namespace PixiEditor.Views;

public interface ICustomTranslatorElement
{
    public void SetTranslationBinding(AvaloniaProperty dependencyProperty, IObservable<string> valueObservable);
    public AvaloniaProperty GetDependencyProperty();
}
