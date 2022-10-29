using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Commands.Templates;

namespace PixiEditor.Views.UserControls;

public partial class ShortcutsTemplateCard : UserControl
{
    public static readonly DependencyProperty TemplateNameProperty = DependencyProperty.Register(
        nameof(TemplateName), typeof(string), typeof(ShortcutsTemplateCard), new PropertyMetadata(default(string)));

    public string TemplateName
    {
        get { return (string)GetValue(TemplateNameProperty); }
        set { SetValue(TemplateNameProperty, value); }
    }

    public static readonly DependencyProperty LogoProperty = DependencyProperty.Register(
        nameof(Logo), typeof(string), typeof(ShortcutsTemplateCard), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HoverLogoProperty = DependencyProperty.Register(
        nameof(HoverLogo), typeof(string), typeof(ShortcutsTemplateCard), new PropertyMetadata(default(string)));

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

    private void OnBorderMouseEnter(object sender, MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(HoverLogo))
        {
            return;
        }
        
        img.Source = new BitmapImage(new Uri(HoverLogo, UriKind.Relative));
    }

    private void BorderMouseLeave(object sender, MouseEventArgs e)
    {
        if (string.IsNullOrEmpty(HoverLogo))
        {
            return;
        }
        
        img.Source = new BitmapImage(new Uri(Logo, UriKind.Relative));
    }
}

