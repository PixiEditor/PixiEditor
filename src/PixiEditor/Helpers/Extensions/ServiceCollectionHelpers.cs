using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.PaletteParsers;
using PixiEditor.Models.IO.PaletteParsers.JascPalFile;
using PixiEditor.Models.Localization;
using PixiEditor.Models.Preferences;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Main;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.Helpers.Extensions;

internal static class ServiceCollectionHelpers
{
    /// <summary>
    /// Adds all the services required to fully run PixiEditor's MainWindow
    /// </summary>
    public static IServiceCollection AddPixiEditor(this IServiceCollection collection, ExtensionLoader extensionLoader)
    {
        return collection
            .AddSingleton<ViewModelMain>()
            .AddSingleton<IPreferences, PreferencesSettings>()
            .AddSingleton<ILocalizationProvider, LocalizationProvider>(x => new LocalizationProvider(extensionLoader))
            // View Models
            .AddSingleton<StylusViewModel>()
            .AddSingleton<WindowViewModel>()
            .AddSingleton<ToolsViewModel>()
            .AddSingleton<FileViewModel>()
            .AddSingleton<UpdateViewModel>()
            .AddSingleton<IoViewModel>()
            .AddSingleton<LayersViewModel>()
            .AddSingleton<ClipboardViewModel>()
            .AddSingleton<UndoViewModel>()
            .AddSingleton<SelectionViewModel>()
            .AddSingleton<ViewOptionsViewModel>()
            .AddSingleton<ColorsViewModel>()
            .AddSingleton<RegistryViewModel>()
            .AddSingleton(static x => new DiscordViewModel(x.GetService<ViewModelMain>(), "764168193685979138"))
            .AddSingleton<DebugViewModel>()
            .AddSingleton<SearchViewModel>()
            .AddSingleton<AdditionalContentViewModel>()
            .AddSingleton(x => new ExtensionsViewModel(x.GetService<ViewModelMain>(), extensionLoader))
            // Controllers
            .AddSingleton<ShortcutController>()
            .AddSingleton<CommandController>()
            .AddSingleton<DocumentManagerViewModel>()
            // Tools
            .AddSingleton<ToolViewModel, MoveViewportToolViewModel>()
            .AddSingleton<ToolViewModel, RotateViewportToolViewModel>()
            .AddSingleton<ToolViewModel, MoveToolViewModel>()
            .AddSingleton<ToolViewModel, PenToolViewModel>()
            .AddSingleton<ToolViewModel, SelectToolViewModel>()
            .AddSingleton<ToolViewModel, MagicWandToolViewModel>()
            .AddSingleton<ToolViewModel, LassoToolViewModel>()
            .AddSingleton<ToolViewModel, FloodFillToolViewModel>()
            .AddSingleton<ToolViewModel, LineToolViewModel>()
            .AddSingleton<ToolViewModel, EllipseToolViewModel>()
            .AddSingleton<ToolViewModel, RectangleToolViewModel>()
            .AddSingleton<ToolViewModel, EraserToolViewModel>()
            .AddSingleton<ToolViewModel, ColorPickerToolViewModel>()
            .AddSingleton<ToolViewModel, BrightnessToolViewModel>()
            .AddSingleton<ToolViewModel, ZoomToolViewModel>()
            // Palette Parsers
            .AddSingleton<PaletteFileParser, JascFileParser>()
            .AddSingleton<PaletteFileParser, ClsFileParser>()
            .AddSingleton<PaletteFileParser, PngPaletteParser>()
            .AddSingleton<PaletteFileParser, PaintNetTxtParser>()
            .AddSingleton<PaletteFileParser, HexPaletteParser>()
            .AddSingleton<PaletteFileParser, GimpGplParser>()
            .AddSingleton<PaletteFileParser, PixiPaletteParser>()
            // Palette data sources
            .AddSingleton<PaletteListDataSource, LocalPalettesFetcher>();
    }

    public static IServiceCollection AddExtensionServices(this IServiceCollection collection) =>
        collection.AddSingleton<IWindowProvider, WindowProvider>()
            .AddSingleton<IPaletteProvider, PaletteProvider>();
}
