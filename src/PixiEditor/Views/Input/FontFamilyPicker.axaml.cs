using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Text;
using PixiEditor.Models.Controllers;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Input;

public partial class FontFamilyPicker : UserControl
{
    private int selectedIndex;

    public static readonly StyledProperty<ICommand> UploadFontCommandProperty =
        AvaloniaProperty.Register<FontFamilyPicker, ICommand>(
            nameof(UploadFontCommand));

    public static readonly StyledProperty<ObservableCollection<FontFamilyName>> FontsProperty = AvaloniaProperty.Register<FontFamilyPicker, ObservableCollection<FontFamilyName>>(
        nameof(Fonts));

    public static readonly StyledProperty<FontFamilyName> SelectedFontFamilyProperty = AvaloniaProperty.Register<FontFamilyPicker, FontFamilyName>(
        nameof(SelectedFontFamily));

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

    public static readonly StyledProperty<int> FontIndexProperty = AvaloniaProperty.Register<FontFamilyPicker, int>(
        nameof(FontIndex));

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

    static FontFamilyPicker()
    {
        FontIndexProperty.Changed.AddClassHandler<FontFamilyPicker>((sender, e) =>
        {
            if (e.NewValue is int newIndex)
            {
                sender.FontIndex = newIndex;
                if (newIndex < 0 || newIndex >= sender.Fonts.Count)
                {
                    sender.FontIndex = sender.Fonts.IndexOf(sender.SelectedFontFamily);
                    return;
                }

                sender.SelectedFontFamily = sender.Fonts[newIndex];
            }
        });
    }

    public FontFamilyPicker()
    {
        InitializeComponent();
        UploadFontCommand = new AsyncRelayCommand(UploadFont);
        Fonts = new ObservableCollection<FontFamilyName>(FontLibrary.AllFonts);
        FontLibrary.FontAdded += (font) => Fonts.Add(font);
        SelectedFontFamily = Fonts[0];
        FontIndex = 0;
    }

    private async Task UploadFont()
    {
        FilePickerFileType[] filter =
        [
            new FilePickerFileType(new LocalizedString("FONT_FILES"))
            {
                Patterns = new List<string> { "*.ttf", "*.otf" }
            },
            new FilePickerFileType("TrueType Font") { Patterns = new List<string> { "*.ttf" } },
            new FilePickerFileType("OpenType Font") { Patterns = new List<string> { "*.otf" } },
        ];

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = filter });

            if (dialog.Count == 0)
                return;

            var fontPath = dialog[0];
            FontFamilyName familyName =
                new FontFamilyName(fontPath.Path, Path.GetFileNameWithoutExtension(fontPath.Name));
            FontLibrary.TryAddCustomFont(familyName);

            FontIndex = Fonts.IndexOf(familyName);
        }
    }
}
