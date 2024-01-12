using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.AppExtensions;
using PixiEditor.AvaloniaUI.Models.AppExtensions.Services;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
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
        AddPixiEditor(this IServiceCollection collection, ExtensionLoader extensionLoader)
    {
        return collection
            .AddSingleton<ViewModelMain>()
            .AddSingleton<IPreferences, PreferencesSettings>()
            .AddSingleton<ILocalizationProvider, LocalizationProvider>(x => new LocalizationProvider(extensionLoader))

            // View Models
            .AddSingleton<ToolsViewModel>()
            .AddSingleton<IToolsHandler, ToolsViewModel>(x => x.GetRequiredService<ToolsViewModel>())
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
            .AddSingleton<IColorsHandler, ColorsViewModel>(x => x.GetRequiredService<ColorsViewModel>())
            .AddSingleton<RegistryViewModel>()
            .AddSingleton(static x => new DiscordViewModel(x.GetService<ViewModelMain>(), "764168193685979138"))
            .AddSingleton<DebugViewModel>()
            .AddSingleton<SearchViewModel>()
            .AddSingleton<ISearchHandler, SearchViewModel>(x => x.GetRequiredService<SearchViewModel>())
            .AddSingleton<AdditionalContentViewModel>()
            .AddSingleton<LayoutDockViewModel>()
            .AddSingleton(x => new ExtensionsViewModel(x.GetService<ViewModelMain>(), extensionLoader))
            // Controllers
            .AddSingleton<ShortcutController>()
            .AddSingleton<CommandController>()
            .AddSingleton<DocumentManagerViewModel>()
            // Tools
            .AddSingleton<IToolHandler, MoveViewportToolViewModel>()
            .AddSingleton<IToolHandler, RotateViewportToolViewModel>()
            .AddSingleton<IMoveToolHandler, MoveToolViewModel>()
            .AddSingleton<IToolHandler, MoveToolViewModel>(x => (MoveToolViewModel)x.GetService<IMoveToolHandler>())
            .AddSingleton<IPenToolHandler, PenToolViewModel>()
            .AddSingleton<IToolHandler, PenToolViewModel>(x => (PenToolViewModel)x.GetService<IPenToolHandler>())
            .AddSingleton<ISelectToolHandler, SelectToolViewModel>()
            .AddSingleton<IToolHandler, SelectToolViewModel>(x => (SelectToolViewModel)x.GetService<ISelectToolHandler>())
            .AddSingleton<IMagicWandToolHandler, MagicWandToolViewModel>()
            .AddSingleton<IToolHandler, MagicWandToolViewModel>(x => (MagicWandToolViewModel)x.GetService<IMagicWandToolHandler>())
            .AddSingleton<ILassoToolHandler, LassoToolViewModel>()
            .AddSingleton<IToolHandler, LassoToolViewModel>(x => (LassoToolViewModel)x.GetService<ILassoToolHandler>())
            .AddSingleton<IFloodFillToolHandler, FloodFillToolViewModel>()
            .AddSingleton<IToolHandler, FloodFillToolViewModel>(x => (FloodFillToolViewModel)x.GetService<IFloodFillToolHandler>())
            .AddSingleton<ILineToolHandler, LineToolViewModel>()
            .AddSingleton<IToolHandler, LineToolViewModel>(x => (LineToolViewModel)x.GetService<ILineToolHandler>())
            .AddSingleton<IEllipseToolHandler, EllipseToolViewModel>()
            .AddSingleton<IToolHandler, EllipseToolViewModel>(x => (EllipseToolViewModel)x.GetService<IEllipseToolHandler>())
            .AddSingleton<IRectangleToolHandler, RectangleToolViewModel>()
            .AddSingleton<IToolHandler, RectangleToolViewModel>(x => (RectangleToolViewModel)x.GetService<IRectangleToolHandler>())
            .AddSingleton<IEraserToolHandler, EraserToolViewModel>()
            .AddSingleton<IToolHandler, EraserToolViewModel>(x => (EraserToolViewModel)x.GetService<IEraserToolHandler>())
            .AddSingleton<IColorPickerHandler, ColorPickerToolViewModel>()
            .AddSingleton<IToolHandler, ColorPickerToolViewModel>(x => (ColorPickerToolViewModel)x.GetService<IColorPickerHandler>())
            .AddSingleton<IBrightnessToolHandler, BrightnessToolViewModel>()
            .AddSingleton<IToolHandler, BrightnessToolViewModel>(x => (BrightnessToolViewModel)x.GetService<IBrightnessToolHandler>())
            .AddSingleton<IToolHandler, ZoomToolViewModel>()
            // Palette Parsers
            .AddSingleton<PaletteFileParser, JascFileParser>()
            .AddSingleton<PaletteFileParser, ClsFileParser>()
            .AddSingleton<PaletteFileParser, DeluxePaintParser>()
            .AddSingleton<PaletteFileParser, CorelDrawPalParser>()
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
