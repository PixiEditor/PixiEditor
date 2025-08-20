using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Extensions;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.Input;
using PixiEditor.UI.Common.Behaviors;

namespace PixiEditor.Views.Main.CommandSearch;
#nullable enable
internal partial class CommandSearchControl : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<string> SearchTermProperty =
        AvaloniaProperty.Register<CommandSearchControl, string>(
            nameof(SearchTerm));

    public string SearchTerm
    {
        get => GetValue(SearchTermProperty);
        set => SetValue(SearchTermProperty, value);
    }

    public static readonly StyledProperty<bool> SelectAllProperty =
        AvaloniaProperty.Register<CommandSearchControl, bool>(
            nameof(SelectAll));

    public bool SelectAll
    {
        get => GetValue(SelectAllProperty);
        set => SetValue(SelectAllProperty, value);
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedResult)));
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

    static CommandSearchControl()
    {
        SearchTermProperty.Changed.Subscribe(OnSearchTermChange);
        IsVisibleProperty.Changed.Subscribe(OnIsVisibleChanged);
    }

    public CommandSearchControl()
    {
        InitializeComponent();

        ButtonClickedCommand = new RelayCommand(() =>
        {
            Hide();
            MouseSelectedResult?.Execute();
            MouseSelectedResult = null;
        });

        PointerPressed += OnPointerDown;
        KeyDown += OnPreviewKeyDown;
        Loaded += (_, _) => UpdateSearchResults();
    }


    private static void OnIsVisibleChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not CommandSearchControl control) return;
        if (e.NewValue.Value)
        {
            Dispatcher.UIThread.Post(
                () =>
                {
                    control.textBox.Focus();
                    control.UpdateSearchResults();

                    // TODO: Mouse capture
                    /*Mouse.Capture(this, CaptureMode.SubTree);*/

                    if (!control.SelectAll)
                    {
                        control.textBox.CaretIndex = control.SearchTerm?.Length ?? 0;
                    }
                }, DispatcherPriority.Input);
        }
    }

    private void OnPointerDown(object sender, PointerPressedEventArgs e)
    {
        var pos = e.GetPosition(this);
        bool outside = pos.X < 0 || pos.Y < 0 || pos.X > Bounds.Width || pos.Y > Bounds.Height;
        if (outside)
            Hide();
    }

    private void UpdateSearchResults()
    {
        Results.Clear();
        (List<SearchResult> newResults, List<string> warnings) =
            CommandSearchControlHelper.ConstructSearchResults(SearchTerm);
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
        // TODO: This
        /*FocusManager.SetFocusedElement(FocusManager.GetFocusScope(textBox), null);
        Keyboard.ClearFocus();*/
        IsVisible = false;
        TextBoxFocusBehavior.FallbackFocusElement.Focus();
        //ReleaseMouseCapture();
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = true;

        OneOf<Color, Error, None> result;

        if (e.Key == Key.Enter && SelectedResult is not null)
        {
            Hide();
            SelectedResult.Execute();
            SelectedResult = null;
        }
        else if (e.Key is Key.Down or Key.PageDown)
        {
            MoveSelection(NextToDirection.Forwards);
        }
        else if (e.Key is Key.Up or Key.PageUp)
        {
            MoveSelection(NextToDirection.Backwards);
        }
        else if (e.Key == Key.Escape ||
                 CommandController.Current.Commands["PixiEditor.Search.Toggle"].Shortcut
                 == new KeyCombination(e.Key, e.KeyModifiers))
        {
            Hide();
        }
        else if (e.Key == Key.R && e.KeyModifiers == KeyModifiers.Control)
        {
            SearchTerm = "rgb(,,)";
            textBox.CaretIndex = 4;
            /*TODO: Validate below, length should be 0*/
            textBox.SelectionStart = 4;
            textBox.SelectionEnd = 4;
        }
        else if (e.Key == Key.Space && SearchTerm.StartsWith("rgb") && textBox.CaretIndex > 0 &&
                 char.IsDigit(SearchTerm[textBox.CaretIndex - 1]))
        {
            var prev = textBox.CaretIndex;
            if (SearchTerm.Length == textBox.CaretIndex || SearchTerm[textBox.CaretIndex] != ',')
            {
                SearchTerm = SearchTerm.Insert(textBox.CaretIndex, ",");
            }

            textBox.CaretIndex = prev + 1;
        }
        else if (e is { Key: Key.S, KeyModifiers: KeyModifiers.Control } &&
                 (result = CommandSearchControlHelper.MaybeParseColor(SearchTerm)).IsT0)
        {
            SwitchColor(result.AsT0);
        }
        else if (e is { Key: Key.D, KeyModifiers: KeyModifiers.Control })
        {
            SearchTerm = "~/Documents/";
            textBox.CaretIndex = SearchTerm.Length;
        }
        else if (e is { Key: Key.P, KeyModifiers: KeyModifiers.Control })
        {
            SearchTerm = "~/Pictures/";
            textBox.CaretIndex = SearchTerm.Length;
        }
        else
        {
            e.Handled = false;
        }
    }

    private void SwitchColor(Color color)
    {
        if (SearchTerm.StartsWith('#'))
        {
            if (color.A == 255)
            {
                SearchTerm = $"rgb({color.R},{color.G},{color.B})";
                textBox.CaretIndex = 4;
            }
            else
            {
                SearchTerm = $"rgba({color.R},{color.G},{color.B},{color.A})";
                textBox.CaretIndex = 5;
            }
        }
        else
        {
            if (color.A == 255)
            {
                SearchTerm = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                textBox.CaretIndex = 1;
            }
            else
            {
                SearchTerm = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                textBox.CaretIndex = 1;
            }
        }
    }

    private void MoveSelection(NextToDirection direction)
    {
        if (SelectedResult is null)
        {
            SelectedResult = Results.FirstOrDefault(x => x.CanExecute);
            return;
        }

        var newIndex = Results.IndexOf(SelectedResult) + (int)direction;
        newIndex = (newIndex % Results.Count + Results.Count) % Results.Count;

        SelectedResult = Results.IndexOrNextInDirection(x => x.CanExecute, newIndex, direction);

        newIndex = Results.IndexOf(SelectedResult);
        itemscontrol.ContainerFromIndex(newIndex)?.BringIntoView();
    }

    private void SearchResult_MouseMove(object sender, PointerEventArgs e)
    {
        var searchResult = ((SearchResultControl)sender).DataContext as SearchResult;
        MouseSelectedResult = searchResult;
    }

    private static void OnSearchTermChange(AvaloniaPropertyChangedEventArgs<string> e)
    {
        CommandSearchControl control = ((CommandSearchControl)e.Sender);
        control.UpdateSearchResults();
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        Hide();
    }

    private void MainGrid_OnTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
    }
}
