using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.DataHolders;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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

    private string warnings = "";
    public string Warnings
    {
        get => warnings;
        set
        {
            warnings = value;
            PropertyChanged?.Invoke(this, new(nameof(Warnings)));
            PropertyChanged?.Invoke(this, new(nameof(HasWarnings)));
        }
    }

    public bool HasWarnings => Warnings != string.Empty;
    public RelayCommand ButtonClickedCommand { get; }

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

    private SearchResult? mouseSelectedResult;
    public SearchResult? MouseSelectedResult
    {
        get => mouseSelectedResult;
        private set
        {
            if (mouseSelectedResult is not null)
                mouseSelectedResult.IsMouseSelected = false;
            if (value is not null)
                value.IsMouseSelected = true;
            mouseSelectedResult = value;
        }
    }

    public ObservableCollection<SearchResult> Results { get; } = new();

    public CommandSearchControl()
    {
        ButtonClickedCommand = new RelayCommand(_ =>
        {
            Hide();
            MouseSelectedResult?.Execute();
            MouseSelectedResult = null;
        });

        InitializeComponent();
        IsVisibleChanged += (_, args) =>
        {
            if (IsVisible)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    textBox.Focus();
                    UpdateSearchResults();
                    Mouse.Capture(this, CaptureMode.SubTree);
                });
            }
        };

        MouseDown += OnMouseDown;
        PreviewKeyDown += OnPreviewKeyDown;
        Loaded += (_, _) => UpdateSearchResults();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(this);
        bool outside = pos.X < 0 || pos.Y < 0 || pos.X > ActualWidth || pos.Y > ActualHeight;
        if (outside)
            Hide();
    }

    private void UpdateSearchResults()
    {
        Results.Clear();
        (List<SearchResult> newResults, List<string> warnings) = CommandSearchControlHelper.ConstructSearchResults(SearchTerm);
        foreach (var result in newResults)
            Results.Add(result);
        Warnings = warnings.Aggregate(new StringBuilder(), static (builder, item) =>
        {
            builder.AppendLine(item);
            return builder;
        }).ToString();
        SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
    }

    private void Hide()
    {
        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
        Keyboard.ClearFocus();
        Visibility = Visibility.Collapsed;
        ReleaseMouseCapture();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;

        if (e.Key == Key.Enter && SelectedResult is not null)
        {
            Hide();
            SelectedResult.Execute();
            SelectedResult = null;
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
            Hide();
        }
        else if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchTerm = "rgb(,,)";
            textBox.CaretIndex = 4;
            textBox.SelectionLength = 0;
        }
        else if (e.Key == Key.Space && SearchTerm.StartsWith("rgb") && char.IsDigit(SearchTerm[textBox.CaretIndex - 1]))
        {
            var prev = textBox.CaretIndex;
            if (SearchTerm.Length == textBox.CaretIndex || SearchTerm[textBox.CaretIndex] != ',')
            {
                SearchTerm = SearchTerm.Insert(textBox.CaretIndex, ",");
            }
            textBox.CaretIndex = prev + 1;
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
        MouseSelectedResult = searchResult;
    }

    private static void OnSearchTermChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        CommandSearchControl control = ((CommandSearchControl)d);
        control.UpdateSearchResults();
        control.PropertyChanged?.Invoke(control, new PropertyChangedEventArgs(nameof(control.SearchTerm)));
    }
}
