using System.Windows;
using PixiEditor.Localization;

namespace PixiEditor.Views;

public class Translator : UIElement
{
    private static void CurrentOnOnLanguageChanged(DependencyObject obj, Language newLanguage)
    {
        obj.SetValue(ValueProperty, new LocalizedString(GetKey(obj)).Value);
    }

    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key",
        typeof(string),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is string key)
        {
            d.SetValue(ValueProperty, new LocalizedString(key).Value);
            ILocalizationProvider.Current.OnLanguageChanged += (lang) => CurrentOnOnLanguageChanged(d, lang);
        }
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
        "Value",
        typeof(string),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender));

    public static void SetKey(DependencyObject element, string value)
    {
        element.SetValue(KeyProperty, value);
    }
    
    public static string GetKey(DependencyObject element)
    {
        return (string)element.GetValue(KeyProperty);
    }

    public static string GetValue(DependencyObject element)
    {
        return (string)element.GetValue(ValueProperty);
    }
}
