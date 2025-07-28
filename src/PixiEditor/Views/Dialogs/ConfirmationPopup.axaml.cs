using Avalonia;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ConfirmationPopup.xaml
/// </summary>
internal partial class ConfirmationPopup : PixiEditorPopup
{
    public static readonly StyledProperty<bool> ResultProperty =
        AvaloniaProperty.Register<ConfirmationPopup, bool>(nameof(Result), true);

    public static readonly StyledProperty<string> BodyProperty =
        AvaloniaProperty.Register<ConfirmationPopup, string>(nameof(Body), string.Empty);

    public static readonly StyledProperty<LocalizedString> FirstOptionTextProperty =
        AvaloniaProperty.Register<ConfirmationPopup, LocalizedString>(nameof(FirstOptionText), new LocalizedString("YES"));

    public static readonly StyledProperty<LocalizedString> SecondOptionTextProperty =
        AvaloniaProperty.Register<ConfirmationPopup, LocalizedString>(nameof(SecondOptionText), new LocalizedString("NO"));

    public LocalizedString FirstOptionText
    {
        get { return (LocalizedString)GetValue(FirstOptionTextProperty); }
        set { SetValue(FirstOptionTextProperty, value); }
    }

    public LocalizedString SecondOptionText
    {
        get { return (LocalizedString)GetValue(SecondOptionTextProperty); }
        set { SetValue(SecondOptionTextProperty, value); }
    }
    
    public ConfirmationPopup()
    {
        InitializeComponent();
        DataContext = this;
    }


    public bool Result
    {
        get => (bool)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }


    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    [RelayCommand]
    public void SetConfirmationResultAndClose(bool property)
    {
        bool result = property;
        Result = result;
        Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Close(false);
    }
}
