using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.DataHolders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixiEditor.Views.UserControls.CommandSearch;
#nullable enable
public partial class CommandSearchControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty SearchTermProperty =
        DependencyProperty.Register(nameof(SearchTerm), typeof(string), typeof(CommandSearchControl), new PropertyMetadata(OnSearchTermChange));

    public string SearchTerm
    {
        get => (string)GetValue(SearchTermProperty);
        set => SetValue(SearchTermProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private SearchResult? selectedResult;

    public SearchResult? SelectedResult
    {
        get => selectedResult;
        private set
        {
            if (selectedResult is not null)
                selectedResult.IsSelected = false;
            if (value is not null)
                value.IsSelected = true;
            selectedResult = value;
        }
    }

    public ObservableCollection<SearchResult> Results { get; } = new();

    public CommandSearchControl()
    {
        InitializeComponent();
        IsVisibleChanged += (_, args) =>
        {
            if (IsVisible)
                Dispatcher.BeginInvoke(DispatcherPriority.Render, () => textBox.Focus());
        };

        textBox.LostFocus += TextBox_LostFocus;
        textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        Loaded += (_, _) => UpdateSearchResults();
    }

    private void UpdateSearchResults()
    {
        Results.Clear();
        List<SearchResult> newResults = CommandSearchControlHelper.ConstructSearchResults(SearchTerm);
        foreach (var result in newResults)
            Results.Add(result);
        SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
        SelectedResult = null;
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (e.Key == Key.Enter && SelectedResult is not null)
        {
            Visibility = Visibility.Collapsed;
            SelectedResult.Execute();
        }
        else if (e.Key is Key.Down or Key.PageDown)
        {
            MoveSelection(1);
        }
        else if (e.Key is Key.Up or Key.PageUp)
        {
            MoveSelection(-1);
        }
        else if (e.Key == Key.Escape ||
                 CommandController.Current.Commands["PixiEditor.Search.Toggle"].Shortcut
                 == new KeyCombination(e.Key, Keyboard.Modifiers))
        {
            FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
            Keyboard.ClearFocus();
        }
        else
        {
            e.Handled = false;
        }
    }

    private void MoveSelection(int delta)
    {
        if (delta == 0)
            return;
        if (SelectedResult is null)
        {
            SelectedResult = Results.Where(x => x.CanExecute).First();
            return;
        }

        int newIndex = Results.IndexOf(SelectedResult) + delta;
        newIndex = (newIndex % Results.Count + Results.Count) % Results.Count;

        SelectedResult = delta > 0 ? Results.IndexOrNext(x => x.CanExecute, newIndex) : Results.IndexOrPrevious(x => x.CanExecute, newIndex);
    }

    private void Button_MouseMove(object sender, MouseEventArgs e)
    {
        var searchResult = ((Button)sender).DataContext as SearchResult;
        SelectedResult = searchResult;
    }

    private static void OnSearchTermChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CommandSearchControl)d).UpdateSearchResults();
    }
}