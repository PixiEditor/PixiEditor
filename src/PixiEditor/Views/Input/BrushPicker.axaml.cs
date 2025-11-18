using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.Palettes;
using Brush = PixiEditor.Models.BrushEngine.Brush;

namespace PixiEditor.Views.Input;

internal partial class BrushPicker : UserControl
{
    public static readonly StyledProperty<ObservableCollection<Brush>> BrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<Brush>>(
            nameof(Brushes));

    public static readonly StyledProperty<ObservableCollection<Brush>> FilteredBrushesProperty =
        AvaloniaProperty.Register<BrushPicker, ObservableCollection<Brush>>(
            nameof(FilteredBrushes));

    public static readonly StyledProperty<string> SearchTextProperty = AvaloniaProperty.Register<BrushPicker, string>(
        nameof(SearchText));

    public static readonly StyledProperty<int> SelectedSortingProperty = AvaloniaProperty.Register<BrushPicker, int>(
        nameof(SelectedSortingIndex));

    public static readonly StyledProperty<string> SortingDirectionProperty = AvaloniaProperty.Register<BrushPicker, string>(
        nameof(SortingDirection), "ascending");

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

    public ObservableCollection<Brush> FilteredBrushes
    {
        get => GetValue(FilteredBrushesProperty);
        set => SetValue(FilteredBrushesProperty, value);
    }

    public ObservableCollection<Brush> Brushes
    {
        get => GetValue(BrushesProperty);
        set => SetValue(BrushesProperty, value);
    }

    public static readonly StyledProperty<Brush?> SelectedBrushProperty =
        AvaloniaProperty.Register<BrushPicker, Brush?>(
            nameof(SelectedBrush));

    public Brush? SelectedBrush
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
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if(SelectedSortingIndex < 0 || SelectedSortingIndex >= SortingOptions.Length)
        {
            SelectedSortingIndex = 0;
        }
    }

    private void UpdateResults()
    {
        var filtered = new ObservableCollection<Brush>();
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = new ObservableCollection<Brush>(Brushes);
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

        filtered = SelectedSortingIndex switch
        {
            (int)BrushSorting.Alphabetical => new ObservableCollection<Brush>(filtered.OrderBy(b => b.Name)),
            _ => filtered
        };


        bool descending = SortingDirection == "descending";
        if (descending)
        {
            filtered = new ObservableCollection<Brush>(filtered.Reverse());
        }

        FilteredBrushes = filtered;
    }

    [RelayCommand]
    public void SelectBrush(Brush brush)
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
