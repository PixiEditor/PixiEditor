using System.Windows;
using System.Windows.Data;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Views;

public abstract class ExternalProperty
{
    public Type PropertyType { get; set; }

    public Action<DependencyObject, Binding>? SetTranslationBinding { get; set; }
    public Action<DependencyObject, LocalizedString>? SetTranslation { get; set; }

    public ExternalProperty(Type propertyType, Action<DependencyObject, Binding> setTranslationBinding)
    {
        PropertyType = propertyType;
        SetTranslationBinding = setTranslationBinding;
    }

    public ExternalProperty(Type propertyType, Action<DependencyObject, LocalizedString> translationAction)
    {
        PropertyType = propertyType;
        SetTranslation = translationAction;
    }
}

public class ExternalProperty<T> : ExternalProperty
{
    public ExternalProperty(Action<DependencyObject, Binding> setTranslationBinding) : base(typeof(T), setTranslationBinding)
    {
    }

    public ExternalProperty(Action<DependencyObject, LocalizedString> translationAction) : base(typeof(T), translationAction)
    {
    }
}


