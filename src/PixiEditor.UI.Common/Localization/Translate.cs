using Avalonia;
using Avalonia.Markup.Xaml;

namespace PixiEditor.UI.Common.Localization;

public class Translate : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    private AvaloniaObject targetObject;
    private AvaloniaProperty targetProperty;

    public Translate()
    {
        if (ILocalizationProvider.Current == null)
        {
            ILocalizationProvider.OnLocalizationProviderChanged += (provider) =>
            {
                ILocalizationProvider.Current.OnLanguageChanged += (lang) => LanguageChanged();
            };
            return;
        }

        ILocalizationProvider.Current.OnLanguageChanged += (lang) => LanguageChanged();
    }


    public override object ProvideValue(IServiceProvider provider)
    {
        if (targetObject == null)
        {
            var target = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget))!;
            targetObject = target.TargetObject as AvaloniaObject;
            targetProperty = target.TargetProperty as AvaloniaProperty;
        }

        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        return new LocalizedString(Key).Value;
    }

    private void LanguageChanged()
    {
        if (targetObject != null && targetProperty != null)
        {
            var newValue = new LocalizedString(Key).Value;
            targetObject.SetValue(targetProperty, newValue);
        }
    }
}
