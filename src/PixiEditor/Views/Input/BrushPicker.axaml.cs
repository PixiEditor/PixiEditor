using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models.Palettes;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.BrushSystem;
using Brush = PixiEditor.Models.BrushEngine.Brush;

namespace PixiEditor.Views.Input;

internal partial class BrushPicker : UserControl
{
    public static readonly StyledProperty<ObservableCollection<BrushViewModel>> BrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<BrushViewModel>>(
            nameof(Brushes));

    public static readonly StyledProperty<ObservableCollection<BrushViewModel>> FilteredBrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<BrushViewModel>>(
            nameof(FilteredBrushes));

    public static readonly StyledProperty<string> SearchTextProperty = AvaloniaProperty.Register<BrushPicker, string>(
        nameof(SearchText));

    public static readonly StyledProperty<int> SelectedSortingProperty = AvaloniaProperty.Register<BrushPicker, int>(
        nameof(SelectedSortingIndex));

    public static readonly StyledProperty<string> SortingDirectionProperty =
        AvaloniaProperty.Register<BrushPicker, string>(
            nameof(SortingDirection), "ascending");

    public static readonly StyledProperty<bool> IsGridViewProperty = AvaloniaProperty.Register<BrushPicker, bool>(
        nameof(IsGridView));

    public static readonly StyledProperty<ObservableCollection<string>> CategoriesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<string>>(
            nameof(Categories));

    public ObservableCollection<string> Categories
    {
        get => GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    public bool IsGridView
    {
        get => GetValue(IsGridViewProperty);
        set => SetValue(IsGridViewProperty, value);
    }

    public string SortingDirection
    {
        get => GetValue(SortingDirectionProperty);
        set => SetValue(SortingDirectionProperty, value);
    }

    public int SelectedSortingIndex
    {
        get => GetValue(SelectedSortingProperty);
        set => SetValue(SelectedSortingProperty, value);
    }

    public BrushSorting[] SortingOptions { get; } = (BrushSorting[])System.Enum.GetValues(typeof(BrushSorting));

    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public ObservableCollection<BrushViewModel> FilteredBrushes
    {
        get => GetValue(FilteredBrushesProperty);
        set => SetValue(FilteredBrushesProperty, value);
    }

    public ObservableCollection<BrushViewModel> Brushes
    {
        get => GetValue(BrushesProperty);
        set => SetValue(BrushesProperty, value);
    }

    public static readonly StyledProperty<BrushViewModel?> SelectedBrushProperty =
        AvaloniaProperty.Register<BrushPicker, BrushViewModel?>(
            nameof(SelectedBrush));

    public BrushViewModel? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    static BrushPicker()
    {
        BrushesProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            if (x.SelectedBrush == null && x.Brushes.Count > 0)
            {
                x.SelectedBrush = x.Brushes[0];
            }

            x.UpdateTags();
            x.UpdateResults();
        });

        SearchTextProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            x.UpdateResults();
        });

        SelectedSortingProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            x.UpdateResults();
        });

        SortingDirectionProperty.Changed.AddClassHandler<BrushPicker>((x, e) =>
        {
            x.UpdateResults();
        });
    }

    public BrushPicker()
    {
        InitializeComponent();
        Categories = new ObservableCollection<string>();
        SelectionText.Text = new LocalizedString("ALL");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (SelectedSortingIndex < 0 || SelectedSortingIndex >= SortingOptions.Length)
        {
            SelectedSortingIndex = 0;
        }

        PopupToggle.Flyout.Opened += Flyout_Opened;
        var options = new ObservableCollection<string>(Categories);
        options.Insert(0, "ALL");
        options.Insert(0, "NONE");
        SelectCategoriesListBox.ItemsSource = options;
        SelectCategoriesListBox.SelectionChanged += SelectCategoriesListBoxOnSelectionChanged;

        SelectCategoriesListBox.SelectAll();

        IPreferences.Current.AddCallback(PreferencesConstants.FavouriteBrushes, OnFaviouritesChanged);
    }

    private void OnFaviouritesChanged(string s, object o)
    {
        UpdateResults();
    }

    private void UpdateTags()
    {
        Categories.Clear();
        foreach (var brush in Brushes)
        {
            foreach (var tag in brush.Brush.Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag) && !Categories.Contains(tag))
                {
                    Categories.Add(tag);
                }
            }
        }

        Categories.Add("UNTAGGED");

        Categories = new ObservableCollection<string>(Categories.OrderBy(c => c));
    }

    private void SelectCategoriesListBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems != null && e.AddedItems.Contains("ALL"))
        {
            SelectCategoriesListBox.SelectedItems = Categories.ToList();
        }
        else if (e.AddedItems != null && e.AddedItems.Contains("NONE"))
        {
            SelectCategoriesListBox.UnselectAll();
        }

        if (SelectCategoriesListBox.SelectedItems.Contains("NONE"))
        {
            SelectCategoriesListBox.SelectedItems.Remove("NONE");
        }

        if (Categories.Count == SelectCategoriesListBox.SelectedItems.Count)
        {
            SelectionText.Text = new LocalizedString("ALL");
        }
        else if (SelectCategoriesListBox.SelectedItems.Count == 0)
        {
            SelectionText.Text = new LocalizedString("NONE");
        }
        else if (SelectCategoriesListBox.SelectedItems.Count == 1)
        {
            SelectionText.Text = new LocalizedString(SelectCategoriesListBox.SelectedItems[0].ToString());
        }
        else
        {
            SelectionText.Text =
                new LocalizedString("SELECTED_CATEGORIES", SelectCategoriesListBox.SelectedItems.Count);
        }

        UpdateResults();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        PopupToggle.Flyout.Opened -= Flyout_Opened;
        SelectCategoriesListBox.SelectionChanged -= SelectCategoriesListBoxOnSelectionChanged;
        IPreferences.Current.RemoveCallback(PreferencesConstants.FavouriteBrushes, OnFaviouritesChanged);
    }

    private void Flyout_Opened(object? sender, EventArgs e)
    {
        int index = SelectedSortingIndex;
        SelectedSortingIndex = -1;
        SelectedSortingIndex = index;
    }

    private void UpdateResults()
    {
        var filtered = new ObservableCollection<BrushViewModel>();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = new ObservableCollection<BrushViewModel>(Brushes);
        }
        else
        {
            var lowerSearch = SearchText?.ToLowerInvariant();
            foreach (var brush in Brushes)
            {
                if (lowerSearch == null || brush.Name.ToLowerInvariant().Contains(lowerSearch))
                {
                    filtered.Add(brush);
                }
            }
        }

        var selectedTags = SelectCategoriesListBox.SelectedItems.Cast<string>().ToList();
        if (selectedTags.Count == 0 && Categories.Count > 0)
        {
            FilteredBrushes = new ObservableCollection<BrushViewModel>();
            return;
        }

        if (selectedTags.Count == Categories.Count)
        {
            FilteredBrushes = filtered;
        }
        else
        {
            filtered = new ObservableCollection<BrushViewModel>(
                filtered.Where(b =>
                    selectedTags.Any(tag => b.Brush.Tags.Contains(tag) || tag == "UNTAGGED" && (!b.Brush.Tags.Any()))));
        }

        bool descending = SortingDirection == "descending";

        filtered = SelectedSortingIndex switch
        {
            (int)BrushSorting.Alphabetical => new ObservableCollection<BrushViewModel>(descending
                ? filtered.OrderByDescending(b => b.Name)
                : filtered.OrderBy(b => b.Name)),

            _ => new ObservableCollection<BrushViewModel>(descending
                ? filtered.Reverse().OrderByDescending(b => b.IsFavourite)
                : filtered.OrderByDescending(b => b.IsFavourite)),
        };

        FilteredBrushes = filtered;
    }

    [RelayCommand]
    public void SelectBrush(BrushViewModel brush)
    {
        SelectedBrush = brush;
        PopupToggle.IsChecked = false;
        PopupToggle.Flyout.Hide();
    }
}

public enum BrushSorting
{
    Default = 0,
    Alphabetical,
}
