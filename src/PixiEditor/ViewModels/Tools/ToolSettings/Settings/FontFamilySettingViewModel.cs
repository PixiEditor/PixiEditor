using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Text;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.UserPreferences;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class FontFamilySettingViewModel : Setting<FontFamilyName>
{
    private ObservableCollection<FontFamilyName> allFonts;
    private int selectedIndex;
    private int previewFontIndex = -1;

    public event Action<FontFamilyName?> PreviewFontFamilyChanged;

    public int FontIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            SetProperty(ref selectedIndex, value);

            if (Fonts?.Count > 0 && value >= 0 && value < Fonts.Count)
            {
                Value = Fonts[value];
            }
            else if (Fonts?.Count is null or 0)
            {
                Value = FontLibrary.DefaultFontFamily;
            }
        }
    }

    public int PreviewFontIndex
    {
        get => previewFontIndex;
        set
        {
            if (!SetProperty(ref previewFontIndex, value))
            {
                return;
            }

            if (Fonts?.Count > 0 && value >= 0 && value < Fonts.Count)
            {
                PreviewFontFamilyChanged?.Invoke(Fonts[value]);
                return;
            }

            PreviewFontFamilyChanged?.Invoke(null);
        }
    }

    public ObservableCollection<FontFamilyName> Fonts
    {
        get => allFonts;
        set => SetProperty(ref allFonts, value);
    }


    public FontFamilySettingViewModel(string name, string displayName) : base(name)
    {
        Label = displayName;
    }
}
