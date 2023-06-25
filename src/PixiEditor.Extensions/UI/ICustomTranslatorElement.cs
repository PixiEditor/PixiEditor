using System.Windows;
using System.Windows.Data;

namespace PixiEditor.Views;

public interface ICustomTranslatorElement
{
    public void SetTranslationBinding(DependencyProperty dependencyProperty, Binding binding);
    public DependencyProperty GetDependencyProperty();
}
