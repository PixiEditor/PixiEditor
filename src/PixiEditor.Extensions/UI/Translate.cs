using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Extensions.UI;

public class Translate : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    private AvaloniaObject targetObject;
    private AvaloniaProperty targetProperty;

    public Translate()
    {
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
