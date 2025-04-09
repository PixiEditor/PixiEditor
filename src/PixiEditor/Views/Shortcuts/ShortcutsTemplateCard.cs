using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using PixiEditor.Helpers.Converters;

namespace PixiEditor.Views.Shortcuts;

public partial class ShortcutsTemplateCard : TemplatedControl
{
    public static readonly StyledProperty<string> TemplateNameProperty =
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(TemplateName));

    public string TemplateName
    {
        get { return (string)GetValue(TemplateNameProperty); }
        set { SetValue(TemplateNameProperty, value); }
    }

    public static readonly StyledProperty<string> LogoProperty =
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(Logo));

    public static readonly StyledProperty<string> HoverLogoProperty =
        AvaloniaProperty.Register<ShortcutsTemplateCard, string>(nameof(HoverLogo));

    public static readonly StyledProperty<ICommand> PressedCommandProperty = AvaloniaProperty.Register<ShortcutsTemplateCard, ICommand>(
        nameof(PressedCommand));

    public static readonly StyledProperty<object> PressedCommandParameterProperty = AvaloniaProperty.Register<ShortcutsTemplateCard, object>(
        nameof(PressedCommandParameter));

    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<ShortcutsTemplateCard, string>(
        nameof(Icon));

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    public object PressedCommandParameter
    {
        get => GetValue(PressedCommandParameterProperty);
        set => SetValue(PressedCommandParameterProperty, value);
    }

    public ICommand PressedCommand
    {
        get => GetValue(PressedCommandProperty);
        set => SetValue(PressedCommandProperty, value);
    }

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

    static ShortcutsTemplateCard()
    {
        LogoProperty.Changed.Subscribe(OnLogoChanged);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (PressedCommand == null)
            {
                return;
            }

            if (PressedCommand.CanExecute(PressedCommandParameter))
            {
                PressedCommand?.Execute(PressedCommandParameter);
            }
        }
    }

    private static void OnLogoChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is ShortcutsTemplateCard card)
        {
            card.Logo = (string)e.NewValue;
            if (string.IsNullOrEmpty(card.HoverLogo))
            {
                card.HoverLogo = (string)e.NewValue;
            }
        }
    }
}
