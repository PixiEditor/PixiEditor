using Avalonia;

namespace PixiEditor.UI.Common.Localization;

public abstract class ExternalProperty
{
    public Type PropertyType { get; set; }

    public Action<AvaloniaObject, IObservable<string>>? SetTranslationBinding { get; set; }
    public Action<AvaloniaObject, LocalizedString>? SetTranslation { get; set; }

    public ExternalProperty(Type propertyType, Action<AvaloniaObject, IObservable<string>> setTranslationBinding)
    {
        PropertyType = propertyType;
        SetTranslationBinding = setTranslationBinding;
    }

    public ExternalProperty(Type propertyType, Action<AvaloniaObject, LocalizedString> translationAction)
    {
        PropertyType = propertyType;
        SetTranslation = translationAction;
    }
}

public class ExternalProperty<T> : ExternalProperty
{
    public ExternalProperty(Action<AvaloniaObject, IObservable<string>> setTranslationBinding) : base(typeof(T), setTranslationBinding)
    {
    }

    public ExternalProperty(Action<AvaloniaObject, LocalizedString> translationAction) : base(typeof(T), translationAction)
    {
    }
}


