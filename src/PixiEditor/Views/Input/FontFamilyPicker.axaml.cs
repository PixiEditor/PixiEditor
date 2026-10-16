using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Text;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models.Controllers;
using PixiEditor.UI.Common.Behaviors;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels;

namespace PixiEditor.Views.Input;

public partial class FontFamilyPicker : UserControl
{
    private readonly HashSet<string> favouriteFonts;
    private string searchText = string.Empty;
    private bool showOnlyFavourites;

    public static readonly StyledProperty<ICommand> UploadFontCommandProperty =
        AvaloniaProperty.Register<FontFamilyPicker, ICommand>(nameof(UploadFontCommand));

    public static readonly StyledProperty<ObservableCollection<FontFamilyName>> FontsProperty =
        AvaloniaProperty.Register<FontFamilyPicker, ObservableCollection<FontFamilyName>>(nameof(Fonts));

    public static readonly StyledProperty<FontFamilyName> SelectedFontFamilyProperty =
        AvaloniaProperty.Register<FontFamilyPicker, FontFamilyName>(nameof(SelectedFontFamily));

    public static readonly StyledProperty<int> FontIndexProperty =
        AvaloniaProperty.Register<FontFamilyPicker, int>(nameof(FontIndex), -1);

    public static readonly DirectProperty<FontFamilyPicker, ObservableCollection<FontItem>>
        FilteredFontsProperty =
            AvaloniaProperty.RegisterDirect<FontFamilyPicker, ObservableCollection<FontItem>>(
                nameof(FilteredFonts), o => o.FilteredFonts);

    public static readonly DirectProperty<FontFamilyPicker, string> SearchTextProperty =
        AvaloniaProperty.RegisterDirect<FontFamilyPicker, string>(
            nameof(SearchText), o => o.SearchText, (o, v) => o.SearchText = v);

    public static readonly DirectProperty<FontFamilyPicker, bool> ShowOnlyFavouritesProperty =
        AvaloniaProperty.RegisterDirect<FontFamilyPicker, bool>(
            nameof(ShowOnlyFavourites), o => o.ShowOnlyFavourites, (o, v) => o.ShowOnlyFavourites = v);

    public FontFamilyName SelectedFontFamily
    {
        get => GetValue(SelectedFontFamilyProperty);
        set => SetValue(SelectedFontFamilyProperty, value);
    }

    public ObservableCollection<FontFamilyName> Fonts
    {
        get => GetValue(FontsProperty);
        set => SetValue(FontsProperty, value);
    }

    public int FontIndex
    {
        get => GetValue(FontIndexProperty);
        set => SetValue(FontIndexProperty, value);
    }

    public ICommand UploadFontCommand
    {
        get => GetValue(UploadFontCommandProperty);
        set => SetValue(UploadFontCommandProperty, value);
    }

    public ObservableCollection<FontItem> FilteredFonts { get; } = new();

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetAndRaise(SearchTextProperty, ref searchText, value))
                RefreshFilteredFonts();
        }
    }

    public bool ShowOnlyFavourites
    {
        get => showOnlyFavourites;
        set
        {
            if (SetAndRaise(ShowOnlyFavouritesProperty, ref showOnlyFavourites, value))
                RefreshFilteredFonts();
        }
    }

    private bool suppressFavourites;
    private IInputElement? oldFocusedElement;

    static FontFamilyPicker()
    {
        FontIndexProperty.Changed.AddClassHandler<FontFamilyPicker>((sender, e) =>
        {
            if (e.NewValue is not int newIndex)
                return;

            if (sender.Fonts is null || newIndex < 0 || newIndex >= sender.Fonts.Count)
            {
                if (sender.Fonts is not null)
                    sender.SetCurrentValue(FontIndexProperty, sender.Fonts.IndexOf(sender.SelectedFontFamily));
                return;
            }

            sender.SelectedFontFamily = sender.Fonts[newIndex];
        });

        FontsProperty.Changed.AddClassHandler<FontFamilyPicker>((sender, _) =>
        {
            sender.SubscribeToFonts();
            sender.RefreshFilteredFonts();
            sender.SynchronizeSelection();
        });

        SelectedFontFamilyProperty.Changed.AddClassHandler<FontFamilyPicker>((sender, _) =>
            sender.SynchronizeSelection());
    }

    private FontFamilyName? preHoverPreviewFont;
    private FontFamilyName? currentHoverPreviewFont;

    public FontFamilyPicker()
    {
        InitializeComponent();

        UploadFontCommand = new AsyncRelayCommand(UploadFont);

        var favFonts = IPreferences.Current.GetPreference<List<string>>(PreferencesConstants.FavouriteFonts, []);
        favouriteFonts = favFonts.Distinct().ToHashSet();

        Fonts = new ObservableCollection<FontFamilyName>(FontLibrary.AllFonts);

        SubscribeToFonts();
        RefreshFilteredFonts();
        SynchronizeSelection();

        FontLibrary.FontAdded += OnFontAdded;
    }

    private void SubscribeToFonts()
    {
        if (Fonts is null)
            return;

        Fonts.CollectionChanged -= Fonts_CollectionChanged;
        Fonts.CollectionChanged += Fonts_CollectionChanged;
    }

    private void Fonts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshFilteredFonts();
        SynchronizeSelection();
    }

    private void OnFontAdded(FontFamilyName font)
    {
        if (!Fonts.Contains(font))
        {
            Fonts.Add(font);
        }
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (e.OldFocusedElement == null) return;
        oldFocusedElement = e.OldFocusedElement;
    }

    private void SynchronizeSelection()
    {
        if (Fonts is null)
            return;

        int index = SelectedFontFamily.Equals(null) ? -1 : Fonts.IndexOf(SelectedFontFamily);

        if (FontIndex != index)
            SetCurrentValue(FontIndexProperty, index);
    }

    private void RefreshFilteredFonts()
    {
        if (Fonts is null)
            return;

        var results = Fonts
            .Where(font =>
                (string.IsNullOrWhiteSpace(SearchText) ||
                 font.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) &&
                (!ShowOnlyFavourites || favouriteFonts.Contains(GetUniqueFontName(font))))
            .ToArray();

        var resultNames = results.Select(GetUniqueFontName).ToHashSet();

        for (int i = FilteredFonts.Count - 1; i >= 0; i--)
        {
            if (!resultNames.Contains(GetUniqueFontName(FilteredFonts[i].FontFamilyName)))
                FilteredFonts.RemoveAt(i);
        }

        for (int i = 0; i < results.Length; i++)
        {
            var font = results[i];
            var uniqueName = GetUniqueFontName(font);
            var isFavourite = favouriteFonts.Contains(uniqueName);

            int existingIndex = -1;

            for (int j = i; j < FilteredFonts.Count; j++)
            {
                if (GetUniqueFontName(FilteredFonts[j].FontFamilyName) == uniqueName)
                {
                    existingIndex = j;
                    break;
                }
            }

            if (existingIndex == -1)
            {
                var fontItem = new FontItem(font, isFavourite);
                fontItem.PropertyChanged += FontItemOnPropertyChanged;
                FilteredFonts.Insert(i, fontItem);
            }
            else
            {
                var fontItem = FilteredFonts[existingIndex];

                if (existingIndex != i)
                    FilteredFonts.Move(existingIndex, i);

                if (fontItem.IsFavourite != isFavourite)
                    fontItem.IsFavourite = isFavourite;
            }
        }
    }

    private static string GetUniqueFontName(FontFamilyName font)
    {
        return font.Name + (font.FontUri != null ? $"_{font.FontUri.AbsoluteUri}" : "");
    }

    private void FontItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (suppressFavourites || e.PropertyName != nameof(FontItem.IsFavourite) || sender is not FontItem fontItem)
            return;

        suppressFavourites = true;
        try
        {
            ToggleFavourite(fontItem.FontFamilyName);
        }
        finally
        {
            suppressFavourites = false;
        }
    }

    public void ToggleFavourite(FontFamilyName font)
    {
        var favoritesInPreferences =
            IPreferences.Current.GetPreference<List<string>>(PreferencesConstants.FavouriteFonts, []);

        var uniqueFontName = GetUniqueFontName(font);
        if (!favouriteFonts.Add(uniqueFontName))
        {
            favouriteFonts.Remove(uniqueFontName);
            favoritesInPreferences.Remove(uniqueFontName);
        }
        else
        {
            favoritesInPreferences?.Add(uniqueFontName);
        }

        IPreferences.Current.UpdatePreference(PreferencesConstants.FavouriteFonts, favoritesInPreferences);
        RefreshFilteredFonts();
    }

    private void FontItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FontFamilyName font })
            return;

        SelectedFontFamily = font;
        FontIndex = Fonts.IndexOf(font);
        preHoverPreviewFont = null;
        currentHoverPreviewFont = null;

        if (TopLevel.GetTopLevel(this) is not null)
        {
            FontPickerToggle.IsChecked = false;
            var focusElement = oldFocusedElement;
            FontPickerToggle.Flyout.Hide();
            Dispatcher.Post(() =>
            {
                focusElement?.Focus();
            });
        }
    }

    private void Button_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Button { Tag: FontFamilyName font })
            return;

        if (currentHoverPreviewFont == null)
        {
            preHoverPreviewFont = SelectedFontFamily;
            currentHoverPreviewFont = font;
        }

        SelectedFontFamily = font;
        FontIndex = Fonts.IndexOf(font);
    }

    private async Task UploadFont()
    {
        FilePickerFileType[] filter =
        [
            new FilePickerFileType(new LocalizedString("FONT_FILES")) { Patterns = ["*.ttf", "*.otf"] },
            new FilePickerFileType("TrueType Font") { Patterns = ["*.ttf"] },
            new FilePickerFileType("OpenType Font") { Patterns = ["*.otf"] }
        ];

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        var dialog = await desktop.MainWindow!.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { FileTypeFilter = filter });

        if (dialog.Count == 0)
            return;

        var fontPath = dialog[0];
        var familyName = new FontFamilyName(fontPath.Path, Path.GetFileNameWithoutExtension(fontPath.Name));

        FontLibrary.TryAddCustomFont(familyName);
        FontIndex = Fonts.IndexOf(familyName);
    }

    private void Button_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (currentHoverPreviewFont == null)
            return;

        if (preHoverPreviewFont != null)
        {
            SelectedFontFamily = preHoverPreviewFont.Value;
            FontIndex = Fonts.IndexOf(preHoverPreviewFont.Value);
        }

        preHoverPreviewFont = null;
        currentHoverPreviewFont = null;
    }
}

public class FontItem : ObservableObject
{
    private FontFamilyName font;
    private bool isFavourite;

    public FontFamilyName FontFamilyName
    {
        get => font;
        set => SetProperty(ref font, value);
    }

    public bool IsFavourite
    {
        get => isFavourite;
        set => SetProperty(ref isFavourite, value);
    }

    public FontItem(FontFamilyName font, bool isFavourite)
    {
        this.font = font;
        this.isFavourite = isFavourite;
    }
}
