using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using PixiEditor.Localization;

namespace PixiEditor.Views;

public class Translator : UIElement
{
    private static void OnLanguageChanged(DependencyObject obj, Language newLanguage)
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
            LocalizedString localizedString = new(key);
            if(d is TextBox textBox)
            {
                textBox.SetBinding(TextBox.TextProperty, new Binding()
                { 
                    Path = new PropertyPath("(views:Translator.Value)"),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self) 
                });
            }
            else if (d is TextBlock textBlock)
            {
                textBlock.SetBinding(TextBlock.TextProperty, new Binding()
                { 
                    Path = new PropertyPath("(views:Translator.Value)"),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self) 
                });
            }
            else if (d is ContentControl contentControl)
            {
                contentControl.SetBinding(ContentControl.ContentProperty, new Binding()
                { 
                    Path = new PropertyPath("(views:Translator.Value)"),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self) 
                });
            }

            d.SetValue(ValueProperty, localizedString.Value);
            ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChanged(d, lang);
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
