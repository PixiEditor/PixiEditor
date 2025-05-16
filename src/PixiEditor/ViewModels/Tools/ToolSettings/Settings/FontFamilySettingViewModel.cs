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

    public int FontIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            SetProperty(ref selectedIndex, value);

            if (Fonts?.Count > 0)
            {
                Value = Fonts[value];
            }
            else
            {
                Value = FontLibrary.DefaultFontFamily;
            }
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
