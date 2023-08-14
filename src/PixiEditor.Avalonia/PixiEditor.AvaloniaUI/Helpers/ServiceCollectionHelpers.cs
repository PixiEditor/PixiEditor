using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.AppExtensions;
using PixiEditor.AvaloniaUI.Models.AppExtensions.Services;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.IO.PaletteParsers;
using PixiEditor.AvaloniaUI.Models.IO.PaletteParsers.JascPalFile;
using PixiEditor.AvaloniaUI.Models.Localization;
using PixiEditor.AvaloniaUI.Models.Palettes;
using PixiEditor.AvaloniaUI.Models.Preferences;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.AvaloniaUI.ViewModels.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Extensions.Windowing;
using ViewModelMain = PixiEditor.AvaloniaUI.ViewModels.ViewModelMain;

namespace PixiEditor.AvaloniaUI.Helpers;

internal static class ServiceCollectionHelpers
{
    /// <summary>
    /// Adds all the services required to fully run PixiEditor's MainWindow
    /// </summary>
    public static IServiceCollection
        AddPixiEditor(this IServiceCollection collection, ExtensionLoader extensionLoader) => collection
        .AddSingleton<ViewModelMain>()
        .AddSingleton<IPreferences, PreferencesSettings>()
        .AddSingleton<ILocalizationProvider, LocalizationProvider>(x => new LocalizationProvider(extensionLoader))

        // View Models
        .AddSingleton<ToolsViewModel>()
        .AddSingleton<IToolsHandler, ToolsViewModel>()
        .AddSingleton<StylusViewModel>()
        .AddSingleton<WindowViewModel>()
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
        .AddSingleton<ISearchHandler, SearchViewModel>()
        .AddSingleton<SearchViewModel>()
        .AddSingleton<AdditionalContentViewModel>()
        .AddSingleton(x => new ExtensionsViewModel(x.GetService<ViewModelMain>(), extensionLoader))
        // Controllers
        .AddSingleton<ShortcutController>()
        .AddSingleton<CommandController>()
        .AddSingleton<DocumentManagerViewModel>()
        // Tools
        .AddSingleton<IToolHandler, MoveViewportToolViewModel>()
        .AddSingleton<IToolHandler, RotateViewportToolViewModel>()
        .AddSingleton<IToolHandler, MoveToolViewModel>()
        .AddSingleton<IToolHandler, PenToolViewModel>()
        .AddSingleton<IToolHandler, SelectToolViewModel>()
        .AddSingleton<IToolHandler, MagicWandToolViewModel>()
        .AddSingleton<IToolHandler, LassoToolViewModel>()
        .AddSingleton<IToolHandler, FloodFillToolViewModel>()
        .AddSingleton<IToolHandler, LineToolViewModel>()
        .AddSingleton<IToolHandler, EllipseToolViewModel>()
        .AddSingleton<IToolHandler, RectangleToolViewModel>()
        .AddSingleton<IToolHandler, EraserToolViewModel>()
        .AddSingleton<IToolHandler, ColorPickerToolViewModel>()
        .AddSingleton<IToolHandler, BrightnessToolViewModel>()
        .AddSingleton<IToolHandler, ZoomToolViewModel>()
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

    public static IServiceCollection AddExtensionServices(this IServiceCollection collection) =>
        collection.AddSingleton<IWindowProvider, WindowProvider>()
            .AddSingleton<IPaletteProvider, PaletteProvider>();
}
