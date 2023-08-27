using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace PixiEditor.AvaloniaUI.Views.Shortcuts;

public partial class ShortcutsTemplateCard : UserControl
{
    public static readonly StyledProperty<string> TemplateNameProperty = 
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(TemplateName));

    public string TemplateName
    {
        get { return (string)GetValue(TemplateNameProperty); }
        set { SetValue(TemplateNameProperty, value); }
    }

    public static readonly StyledProperty<string>  LogoProperty = 
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(Logo));

    public static readonly StyledProperty<string>  HoverLogoProperty = 
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(HoverLogo));

    public string HoverLogo
    {
        get { return (string)GetValue(HoverLogoProperty); }
        set { SetValue(HoverLogoProperty, value); }
    }
    
    public string Logo
    {
        get { return (string)GetValue(LogoProperty); }
        set { SetValue(LogoProperty, value); }
    }
    
    public ShortcutsTemplateCard()
    {
        InitializeComponent();
    }

    private void OnBorderPointerExited(object? sender, PointerEventArgs e)
    {
        if (string.IsNullOrEmpty(HoverLogo))
        {
            return;
        }
        
        img.Source = new Bitmap(HoverLogo);
    }

    private void OnBorderPointerEntered(object? sender, PointerEventArgs e)
    {
        if (string.IsNullOrEmpty(HoverLogo))
        {
            return;
        }
        
        img.Source = new Bitmap(Logo);
    }
}

