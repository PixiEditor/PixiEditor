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
    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key",
        typeof(string),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, KeyPropertyChangedCallback));

    public static readonly DependencyProperty LocalizedStringProperty = DependencyProperty.RegisterAttached(
        "LocalizedString",
        typeof(LocalizedString),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(LocalizedString), FrameworkPropertyMetadataOptions.AffectsRender, LocalizedStringPropertyChangedCallback));

    public static readonly DependencyProperty EnumProperty = DependencyProperty.RegisterAttached(
        "Enum",
        typeof(object),
        typeof(Translator),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, EnumPropertyChangedCallback));

    public static readonly DependencyProperty TooltipKeyProperty = DependencyProperty.RegisterAttached(
        "TooltipKey", typeof(string), typeof(Translator), 
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender, TooltipKeyPropertyChangedCallback));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
        "Value",
        typeof(string),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TooltipLocalizedStringProperty = DependencyProperty.RegisterAttached(
        "TooltipLocalizedString",
        typeof(LocalizedString),
        typeof(Translator),
        new FrameworkPropertyMetadata(default(LocalizedString), FrameworkPropertyMetadataOptions.AffectsRender, TooltipLocalizedStringPropertyChangedCallback));

    private static void TooltipLocalizedStringPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.SetValue(FrameworkElement.ToolTipProperty, GetTooltipLocalizedString(d).Value);
        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedTooltipString(d, lang);
    }

    private static void OnLanguageChangedTooltipString(DependencyObject dependencyObject, Language lang)
    {
        LocalizedString localizedString = GetTooltipLocalizedString(dependencyObject);
        LocalizedString newLocalizedString = new(localizedString.Key, localizedString.Parameters);

        dependencyObject.SetValue(FrameworkElement.ToolTipProperty, newLocalizedString.Value);
    }
    
    private static void TooltipKeyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.SetValue(FrameworkElement.ToolTipProperty, new LocalizedString(GetTooltipKey(d)).Value);

        if (ILocalizationProvider.Current == null)
        {
            return;
        }
        
        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedTooltipKey(d, lang);
    }

    private static void OnLanguageChangedKey(DependencyObject obj, Language newLanguage)
    {
        string key = GetKey(obj);
        if (key != null)
        {
            UpdateKey(obj, key);
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
            UpdateKey(d, key);
            
            if (ILocalizationProvider.Current == null)
            {
                return;
            }

            ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedKey(d, lang);
        }
    }

    private static void LocalizedStringPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        d.SetValue(KeyProperty, ((LocalizedString)e.NewValue).Key);
    }

    private static void EnumPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue == null)
        {
            d.SetValue(KeyProperty, null);
            return;
        }
        
        d.SetValue(KeyProperty, EnumHelpers.GetDescription(e.NewValue));
    }

    private static void UpdateKey(DependencyObject d, string key)
    {
        var parameters = GetLocalizedString(d).Parameters;
        LocalizedString localizedString = new(key, parameters);
        Binding binding = new()
        {
            Path = new PropertyPath("(0)", ValueProperty),
            RelativeSource = new RelativeSource(RelativeSourceMode.Self)
        };

        if (d is TextBox textBox)
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
        else if (d is Window window)
        {
            window.SetBinding(Window.TitleProperty, binding);
        }
        #if DEBUG
        else if (d is DialogTitleBar)
        {
            throw new ArgumentException($"Use {nameof(DialogTitleBar)}.{nameof(DialogTitleBar.TitleKey)} to set the localization key for the title");
        }
        #endif
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
        #if DEBUG
        else
        {
            throw new ArgumentException($"'{d.GetType().Name}' does not support {nameof(Translator)}.Key");
        }
        #endif

        d.SetValue(ValueProperty, localizedString.Value);
    }

    public static void SetKey(DependencyObject element, string value)
    {
        element.SetValue(KeyProperty, value);
    }

    public static string GetKey(DependencyObject element)
    {
        return (string)element.GetValue(KeyProperty);
    }

    public static void SetLocalizedString(DependencyObject element, LocalizedString value)
    {
        element.SetValue(LocalizedStringProperty, value);
    }

    public static void SetEnum(DependencyObject element, object value)
    {
        element.SetValue(EnumProperty, value);
    }

    public static LocalizedString GetLocalizedString(DependencyObject element)
    {
        return (LocalizedString)element.GetValue(LocalizedStringProperty);
    }
    
    public static string GetValue(DependencyObject element)
    {
        return (string)element.GetValue(ValueProperty);
    }

    public static LocalizedString GetTooltipLocalizedString(DependencyObject element)
    {
        return (LocalizedString)element.GetValue(TooltipLocalizedStringProperty);
    }

    public static void SetTooltipLocalizedString(UIElement element, LocalizedString value)
    {
        element.SetValue(TooltipLocalizedStringProperty, value);
    }
}
