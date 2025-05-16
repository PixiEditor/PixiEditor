using Avalonia;

namespace PixiEditor.UI.Common.Localization;

public interface ICustomTranslatorElement
{
    public void SetTranslationBinding(AvaloniaProperty dependencyProperty, IObservable<string> binding);
    public AvaloniaProperty GetDependencyProperty();
}
