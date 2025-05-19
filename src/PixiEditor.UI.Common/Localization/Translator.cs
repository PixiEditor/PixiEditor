using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Reactive;

namespace PixiEditor.UI.Common.Localization;

public class Translator : Control
{
    public static List<ExternalProperty> ExternalProperties { get; } = new List<ExternalProperty>();

    public static readonly AttachedProperty<string> KeyProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, string>("Key");

    public static readonly AttachedProperty<LocalizedString> LocalizedStringProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, LocalizedString>("LocalizedString");

    public static readonly AttachedProperty<object> EnumProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, object>("Enum");

    public static readonly AttachedProperty<string> TooltipKeyProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, string>("TooltipKey");

    public static readonly AttachedProperty<string> ValueProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, string>("Value");

    public static readonly AttachedProperty<LocalizedString> TooltipLocalizedStringProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, LocalizedString>("TooltipLocalizedString");

    public static readonly AttachedProperty<bool> UseLanguageFlowDirectionProperty =
        AvaloniaProperty.RegisterAttached<Translator, Control, bool>("UseLanguageFlowDirection");

    public static void SetUseLanguageFlowDirection(Control obj, bool value) => obj.SetValue(UseLanguageFlowDirectionProperty, value);
    public static bool GetUseLanguageFlowDirection(Control obj) => obj.GetValue(UseLanguageFlowDirectionProperty);

    static Translator()
    {
        IObserver<AvaloniaPropertyChangedEventArgs<string>> keyObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string>>(KeyPropertyChanged);
        KeyProperty.Changed.Subscribe(keyObserver);

        IObserver<AvaloniaPropertyChangedEventArgs<LocalizedString>> localizedStringObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<LocalizedString>>(LocalizedStringPropertyChanged);
        LocalizedStringProperty.Changed.Subscribe(localizedStringObserver);

        IObserver<AvaloniaPropertyChangedEventArgs<object>> enumObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<object>>(EnumPropertyChanged);
        EnumProperty.Changed.Subscribe(enumObserver);

        IObserver<AvaloniaPropertyChangedEventArgs<string>> tooltipKeyObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string>>(TooltipKeyPropertyChanged);
        TooltipKeyProperty.Changed.Subscribe(tooltipKeyObserver);

        IObserver<AvaloniaPropertyChangedEventArgs<LocalizedString>> tooltipLocalizedStringObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<LocalizedString>>(TooltipLocalizedStringPropertyChanged);
        TooltipLocalizedStringProperty.Changed.Subscribe(tooltipLocalizedStringObserver);

        IObserver<AvaloniaPropertyChangedEventArgs<bool>> useLanguageFlowDirectionObserver = new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(UseLanguageFlowDirectionPropertyChanged);
        UseLanguageFlowDirectionProperty.Changed.Subscribe(useLanguageFlowDirectionObserver);
    }

    private static void UseLanguageFlowDirectionPropertyChanged(AvaloniaPropertyChangedEventArgs<bool> obj)
    {
        if (!obj.NewValue.Value)
        {
            obj.Sender.SetValue(Control.FlowDirectionProperty, FlowDirection.LeftToRight);
            ILocalizationProvider.Current.OnLanguageChanged -= (lang) => OnLanguageChangedFlowDirection(obj.Sender);
        }
        else
        {
            OnLanguageChangedFlowDirection(obj.Sender);
            ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedFlowDirection(obj.Sender);
        }
    }

    private static void OnLanguageChangedFlowDirection(AvaloniaObject objSender)
    {
        objSender.SetValue(Control.FlowDirectionProperty, ILocalizationProvider.Current?.CurrentLanguage.FlowDirection ?? FlowDirection.LeftToRight);
    }

    private static void TooltipLocalizedStringPropertyChanged(AvaloniaPropertyChangedEventArgs<LocalizedString> e)
    {
        e.Sender.SetValue(ToolTip.TipProperty, GetTooltipLocalizedString(e.Sender).Value);
        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedTooltipString(e.Sender, lang);
    }

    private static void OnLanguageChangedTooltipString(AvaloniaObject dependencyObject, Language lang)
    {
        LocalizedString localizedString = GetTooltipLocalizedString(dependencyObject);
        LocalizedString newLocalizedString = new(localizedString.Key, localizedString.Parameters);

        dependencyObject.SetValue(ToolTip.TipProperty, newLocalizedString.Value);
    }

    private static void TooltipKeyPropertyChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        e.Sender.SetValue(ToolTip.TipProperty, new LocalizedString(GetTooltipKey(e.Sender)).Value);

        if (ILocalizationProvider.Current == null)
        {
            return;
        }

        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedTooltipKey(e.Sender, lang);
    }

    private static void OnLanguageChangedKey(AvaloniaObject obj, Language newLanguage)
    {
        string key = GetKey(obj);
        if (key != null)
        {
            UpdateKey(obj, key);
        }
    }

    private static void OnLanguageChangedTooltipKey(AvaloniaObject element, Language lang)
    {
        string tooltipKey = GetTooltipKey(element);
        if (tooltipKey != null)
        {
            element.SetValue(ToolTip.TipProperty, new LocalizedString(tooltipKey).Value);
        }
    }

    public static void SetTooltipKey(AvaloniaObject element, string value)
    {
        element.SetValue(TooltipKeyProperty, value);
    }

    public static string GetTooltipKey(AvaloniaObject element)
    {
        return (string)element.GetValue(TooltipKeyProperty);
    }

    private static void KeyPropertyChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        UpdateKey(e.Sender, e.NewValue.Value);

        if (ILocalizationProvider.Current == null)
        {
            return;
        }

        ILocalizationProvider.Current.OnLanguageChanged += (lang) => OnLanguageChangedKey(e.Sender, lang);
    }

    private static void LocalizedStringPropertyChanged(AvaloniaPropertyChangedEventArgs<LocalizedString> e)
    {
        e.Sender.SetValue(KeyProperty, (e.NewValue.Value).Key);
    }

    private static void EnumPropertyChanged(AvaloniaPropertyChangedEventArgs<object> e)
    {
        if (!e.NewValue.HasValue)
        {
            e.Sender.SetValue(KeyProperty, null);
            return;
        }

        e.Sender.SetValue(KeyProperty, EnumHelpers.GetDescription(e.NewValue));
    }

    private static void UpdateKey(AvaloniaObject d, string key)
    {
        if(key == null) return;
        var parameters = GetLocalizedString(d).Parameters;
        LocalizedString localizedString = new(key, parameters);

        var valueObservable = d.GetObservable(ValueProperty);

        ExternalProperty externalProperty = ExternalProperties.FirstOrDefault(x => x.PropertyType.IsAssignableFrom(d.GetType()));

        if (d is ICustomTranslatorElement customTranslatorElement)
        {
            customTranslatorElement.SetTranslationBinding(customTranslatorElement.GetDependencyProperty(), valueObservable);
        }
        else if (externalProperty != null)
        {
            if (externalProperty.SetTranslationBinding != null)
            {
                externalProperty.SetTranslationBinding(d, valueObservable);
            }
            else
            {
                externalProperty.SetTranslation?.Invoke(d, localizedString);
            }
        }
        else if (d is TextBox textBox)
        {
            textBox.Bind(TextBox.TextProperty, valueObservable);
        }
        else if (d is TextBlock textBlock)
        {
            //textBlock.Bind(TextBlock.TextProperty, binding);
            textBlock.Bind(TextBlock.TextProperty, valueObservable);
        }
        else if (d is Run run)
        {
            run.Bind(Run.TextProperty, valueObservable);
        }
        else if (d is Window window)
        {
            window.Bind(Window.TitleProperty, valueObservable);
        }
        else if (d is HeaderedSelectingItemsControl menuItem)
        {
            menuItem.Bind(HeaderedSelectingItemsControl.HeaderProperty, valueObservable);
        }
        else if (d is HeaderedContentControl headeredContentControl)
        {
            headeredContentControl.Bind(HeaderedContentControl.HeaderProperty, valueObservable);
        }
        else if (d is ContentControl contentControl)
        {
            contentControl.Bind(ContentControl.ContentProperty, valueObservable);
        }
        else if (d is NativeMenuItem nativeMenuItem)
        {
            nativeMenuItem.Bind(NativeMenuItem.HeaderProperty, valueObservable);
        }
#if DEBUG
        else
        {
            throw new ArgumentException($"'{d.GetType().Name}' does not support {nameof(Translator)}.Key");
        }
        #endif

        d.SetValue(ValueProperty, localizedString.Value);
    }

    public static void SetKey(AvaloniaObject element, string value)
    {
        element.SetValue(KeyProperty, value);
    }

    public static string GetKey(AvaloniaObject element)
    {
        return element.GetValue(KeyProperty);
    }

    public static void SetLocalizedString(AvaloniaObject element, LocalizedString value)
    {
        element.SetValue(LocalizedStringProperty, value);
    }

    public static void SetEnum(AvaloniaObject element, object value)
    {
        element.SetValue(EnumProperty, value);
    }

    public static LocalizedString GetLocalizedString(AvaloniaObject element)
    {
        return element.GetValue(LocalizedStringProperty);
    }

    public static string GetValue(AvaloniaObject element)
    {
        return (string)element.GetValue(ValueProperty);
    }

    public static LocalizedString GetTooltipLocalizedString(AvaloniaObject element)
    {
        return element.GetValue(TooltipLocalizedStringProperty);
    }

    public static void SetTooltipLocalizedString(AvaloniaObject element, LocalizedString value)
    {
        element.SetValue(TooltipLocalizedStringProperty, value);
    }
}
