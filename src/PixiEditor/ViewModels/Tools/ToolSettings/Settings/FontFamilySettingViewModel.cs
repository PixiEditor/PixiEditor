using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Text;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.UserPreferences;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class FontFamilySettingViewModel : Setting<FontFamilyName>
{
    private int selectedIndex;


    private ObservableCollection<FontFamilyName> _fonts;

    public ObservableCollection<FontFamilyName> Fonts
    {
        get
        {
            return _fonts;
        }
        set
        {
            SetProperty(ref _fonts, value);
        }
    }


    public int FontIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            SetProperty(ref selectedIndex, value);
            Value = Fonts[value];
        }
    }

    public AsyncRelayCommand UploadFontCommand { get; }

    public FontFamilySettingViewModel(string name, string displayName) : base(name)
    {
        Label = displayName;
        Fonts = new ObservableCollection<FontFamilyName>(FontDomain.AllFonts);
        FontDomain.FontAdded += (font) => Fonts.Add(font); 
        UploadFontCommand = new AsyncRelayCommand(UploadFont);
    }

    private async Task UploadFont()
    {
        FilePickerFileType[] filter =
        [
            new FilePickerFileType(new LocalizedString("FONT_FILES")) { Patterns = new List<string> { "*.ttf", "*.otf" } },
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
            FontFamilyName familyName = new FontFamilyName(fontPath.Path, Path.GetFileNameWithoutExtension(fontPath.Name));
            FontDomain.TryAddCustomFont(familyName);
            
            FontIndex = Fonts.IndexOf(familyName);
        }
    }
}
