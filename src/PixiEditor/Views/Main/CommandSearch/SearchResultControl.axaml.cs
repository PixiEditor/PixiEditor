using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using PixiEditor.Models.Commands.Search;

namespace PixiEditor.Views.Main.CommandSearch;

internal partial class SearchResultControl : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<SearchResult> ResultProperty =
        AvaloniaProperty.Register<SearchResultControl, SearchResult>(
            nameof(Result));

    public static readonly StyledProperty<ICommand> ButtonClickedCommandProperty =
        AvaloniaProperty.Register<SearchResultControl, ICommand>(
            nameof(ButtonClickedCommand));

    public ICommand ButtonClickedCommand
    {
        get => GetValue(ButtonClickedCommandProperty);
        set => SetValue(ButtonClickedCommandProperty, value);
    }

    public SearchResult Result
    {
        get => GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public IImage? EvaluatedIcon { get; private set; }
    public bool CanExecute { get; private set; } = true;

    public new event PropertyChangedEventHandler? PropertyChanged;

    static SearchResultControl()
    {
        ResultProperty.Changed.Subscribe(ResultChanged);
    }
    
    public SearchResultControl()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        EvaluateCanExecute();
        EvaluateIcon();
    }

    private void EvaluateCanExecute()
    {
        CanExecute = Result.CanExecute;
        PropertyChanged?.Invoke(this, new(nameof(CanExecute)));
    }

    private void EvaluateIcon()
    {
        IImage icon = Result.Icon;
        EvaluatedIcon = icon;
        PropertyChanged?.Invoke(this, new(nameof(EvaluatedIcon)));
    }
    
    private void OnResultPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchResult.CanExecute))
        {
            EvaluateCanExecute();
            EvaluateIcon();
        }
    }
    
    private static void ResultChanged(AvaloniaPropertyChangedEventArgs<SearchResult> e)
    {
        if (e.Sender is SearchResultControl control)
        {
            if (e.OldValue.Value != null)
            {
                e.OldValue.Value.PropertyChanged -= control.OnResultPropertyChanged;
            }
            if (e.NewValue.Value != null)
            {
                e.NewValue.Value.PropertyChanged += control.OnResultPropertyChanged;
            }
        }
    }
}
