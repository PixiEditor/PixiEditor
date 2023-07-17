using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Localization;
using PixiEditor.Models.Preferences;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;

namespace PixiEditor.Helpers.Extensions;

internal static class ServiceCollectionHelpers
{
    /// <summary>
    /// Adds all the services required to fully run PixiEditor's MainWindow
    /// </summary>
    public static IServiceCollection
        AddPixiEditor(this IServiceCollection collection, ExtensionLoader extensionLoader) => collection
        .AddSingleton<MainViewModel>()
        .AddSingleton<IPreferences, PreferencesSettings>()
        .AddSingleton<ILocalizationProvider, LocalizationProvider>(x => new LocalizationProvider(extensionLoader))

        // View Models
        .AddSingleton<ToolsViewModel>()
        .AddSingleton<IToolsHandler, ToolsViewModel>()
        /*.AddSingleton<StylusViewModel>()
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
        .AddSingleton<SearchViewModel>()*/
        .AddSingleton<AdditionalContentViewModel>()
        //.AddSingleton(x => new ExtensionsViewModel(x.GetService<ViewModelMain>(), extensionLoader))
        // Controllers
        //.AddSingleton<ShortcutController>()
        .AddSingleton<CommandController>();
        /*.AddSingleton<DocumentManagerViewModel>()
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
        .AddSingleton<PaletteListDataSource, LocalPalettesFetcher>();*/

    public static IServiceCollection AddExtensionServices(this IServiceCollection collection) =>
        collection.AddSingleton<IWindowProvider, WindowProvider>()
            .AddSingleton<IPaletteProvider, PaletteProvider>();
}
