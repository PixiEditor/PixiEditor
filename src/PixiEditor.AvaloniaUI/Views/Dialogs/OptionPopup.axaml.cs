using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class OptionPopup : Window
{
    public static readonly StyledProperty<object> PopupContentProperty =
        AvaloniaProperty.Register<OptionPopup, object>(nameof(PopupContent));

    public object PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<object>> OptionsProperty =
        AvaloniaProperty.Register<OptionPopup, ObservableCollection<object>>(nameof(Options));

    public ObservableCollection<object> Options
    {
        get => GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public static readonly StyledProperty<object?> ResultProperty =
        AvaloniaProperty.Register<OptionPopup, object?>(nameof(Result));

    public object? Result
    {
        get => GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public RelayCommand CancelCommand { get; set; }

    public RelayCommand CloseCommand { get; set; }

    public OptionPopup(string title, object content, ObservableCollection<object> options)
    {
        PopupContent = content;
        Options = options;
        CancelCommand = new RelayCommand(Cancel);
        CloseCommand = new RelayCommand(Close);
        InitializeComponent();
        
        Loaded += OptionPopup_Loaded;
    }

    private void OptionPopup_Loaded(object? sender, EventArgs e)
    {
        InvalidateVisual();
    }

    private void Cancel()
    {
        Result = null;
        Close();
    }

    private void Close(object parameter)
    {
        Result = parameter;
        Close();
    }
}

