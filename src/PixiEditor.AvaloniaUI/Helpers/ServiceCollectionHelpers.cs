using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.ExtensionServices;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.IO.PaletteParsers;
using PixiEditor.AvaloniaUI.Models.IO.PaletteParsers.JascPalFile;
using PixiEditor.AvaloniaUI.Models.Localization;
using PixiEditor.AvaloniaUI.Models.Palettes;
using PixiEditor.AvaloniaUI.Models.Preferences;
using PixiEditor.AvaloniaUI.ViewModels.Dock;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.Menu;
using PixiEditor.AvaloniaUI.ViewModels.Menu.MenuBuilders;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.FlyUI;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.Runtime;
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
            .AddSingleton<AnimationsViewModel>()
            .AddSingleton<IColorsHandler, ColorsViewModel>(x => x.GetRequiredService<ColorsViewModel>())
            .AddSingleton<RegistryViewModel>()
            .AddSingleton(static x => new DiscordViewModel(x.GetService<ViewModelMain>(), "764168193685979138"))
            .AddSingleton<DebugViewModel>()
            .AddSingleton<SearchViewModel>()
            .AddSingleton<ISearchHandler, SearchViewModel>(x => x.GetRequiredService<SearchViewModel>())
            .AddSingleton<AdditionalContentViewModel>()
            .AddSingleton<LayoutManager>()
            .AddSingleton<LayoutViewModel>()
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
            .AddSingleton<IPalettesProvider, PaletteProvider>()
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
            .AddSingleton<PaletteListDataSource, LocalPalettesFetcher>()
            .AddMenuBuilders();
    }

    private static IServiceCollection AddMenuBuilders(this IServiceCollection collection)
    {
        return collection
            .AddSingleton<MenuItemBuilder, RecentFilesMenuBuilder>()
            .AddSingleton<MenuItemBuilder, FileExitMenuBuilder>()
            .AddSingleton<MenuItemBuilder, SymmetryMenuBuilder>()
            .AddSingleton<MenuItemBuilder, OpenDockablesMenuBuilder>()
            .AddSingleton<MenuItemBuilder, ToggleGridLinesMenuBuilder>();
    }

    public static IServiceCollection AddExtensionServices(this IServiceCollection collection, ExtensionLoader loader) =>
        collection.AddSingleton<IWindowProvider, WindowProvider>(x => new WindowProvider(loader, x))
            .AddSingleton<ElementMap>(x =>
            {
                ElementMap elementMap = new ElementMap();
                Assembly[] pixiEditorAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.FullName.StartsWith("PixiEditor")).ToArray();
                foreach (Assembly assembly in pixiEditorAssemblies)
                {
                    elementMap.AddElementsFromAssembly(assembly);
                }

                return elementMap;
            })
            .AddSingleton<IFileSystemProvider, FileSystemProvider>();
}
