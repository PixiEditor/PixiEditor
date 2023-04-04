using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using AvalonDock.Layout;
using PixiEditor.Localization;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views;

public class Translator : UIElement
{
    private static void OnLanguageChangedKey(DependencyObject obj, Language newLanguage)
    {
        string key = GetKey(obj);
        if (key != null)
        {
            obj.SetValue(ValueProperty, new LocalizedString(key).Value);
        }
    }

    private static void OnLanguageChangedTooltipKey(DependencyObject element, Language lang)
    {
        string tooltipKey = GetTooltipKey(element);
        if (tooltipKey != null)
        {
            element.SetValue(FrameworkElement.ToolTipProperty, new LocalizedString(tooltipKey).Value);
        }
    }

    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key",
        typeof(string),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, KeyPropertyChangedCallback));

    public static readonly DependencyProperty TooltipKeyProperty = DependencyProperty.RegisterAttached(
        "TooltipKey", typeof(string), typeof(Translator), 
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, TooltipKeyPropertyChangedCallback));

    private static void TooltipKeyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.SetValue(FrameworkElement.ToolTipProperty, new LocalizedString(GetTooltipKey(d)).Value);
        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedTooltipKey(d, lang);
    }

    public static void SetTooltipKey(DependencyObject element, string value)
    {
        element.SetValue(TooltipKeyProperty, value);
    }

    public static string GetTooltipKey(DependencyObject element)
    {
        return (string)element.GetValue(TooltipKeyProperty);
    }

    private static void KeyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is string key)
        {
            LocalizedString localizedString = new(key);
            Binding binding = new()
            {
                Path = new PropertyPath("(0)", Translator.ValueProperty),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            };

            if(d is TextBox textBox)
            {
                textBox.SetBinding(TextBox.TextProperty, binding);
            }
            else if (d is TextBlock textBlock)
            {
                textBlock.SetBinding(TextBlock.TextProperty, binding);
            }
            else if (d is Run run)
            {
                run.SetBinding(Run.TextProperty, binding);
            }
            else if (d is ContentControl contentControl)
            {
                contentControl.SetBinding(ContentControl.ContentProperty, binding);
            }
            else if (d is HeaderedItemsControl menuItem)
            {
                menuItem.SetBinding(HeaderedItemsControl.HeaderProperty, binding);
            }
            else if (d is LayoutContent layoutContent)
            {
                layoutContent.SetValue(LayoutContent.TitleProperty, localizedString.Value);
            }

            d.SetValue(ValueProperty, localizedString.Value);
            ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedKey(d, lang);
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
