using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AnimationRenderer.FFmpeg;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.FlyUI;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Models.Files;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.IO.PaletteParsers;
using PixiEditor.Models.IO.PaletteParsers.JascPalFile;
using PixiEditor.Models.Localization;
using PixiEditor.Models.Palettes;
using PixiEditor.Models.Preferences;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Menu;
using PixiEditor.ViewModels.Menu.MenuBuilders;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.ViewModels.Tools.Tools;
using ViewModelMain = PixiEditor.ViewModels.ViewModelMain;
using ViewModels_ViewModelMain = PixiEditor.ViewModels.ViewModelMain;

namespace PixiEditor.Helpers;

internal static class ServiceCollectionHelpers
{
    /// <summary>
    /// Adds all the services required to fully run PixiEditor's MainWindow
    /// </summary>
    public static IServiceCollection
        AddPixiEditor(this IServiceCollection collection, ExtensionLoader extensionLoader)
    {
        return collection
            .AddSingleton<ViewModels_ViewModelMain>()
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
            .AddSingleton<NodeGraphManagerViewModel>()
            .AddSingleton<IColorsHandler, ColorsViewModel>(x => x.GetRequiredService<ColorsViewModel>())
            .AddSingleton<RegistryViewModel>()
            .AddSingleton(static x => new DiscordViewModel(x.GetService<ViewModels_ViewModelMain>(), "764168193685979138"))
            .AddSingleton<DebugViewModel>()
            .AddSingleton<SearchViewModel>()
            .AddSingleton<ISearchHandler, SearchViewModel>(x => x.GetRequiredService<SearchViewModel>())
            .AddSingleton<AdditionalContentViewModel>()
            .AddSingleton<LayoutManager>()
            .AddSingleton<LayoutViewModel>()
            .AddSingleton(x => new ExtensionsViewModel(x.GetService<ViewModels_ViewModelMain>(), extensionLoader))
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
            // File types
            .AddSingleton<IoFileType, PixiFileType>()
            .AddSingleton<IoFileType, PngFileType>()
            .AddSingleton<IoFileType, JpegFileType>()
            .AddSingleton<IoFileType, BmpFileType>()
            .AddSingleton<IoFileType, GifFileType>()
            .AddSingleton<IoFileType, Mp4FileType>()
            // Serialization Factories
            .AddSingleton<SerializationFactory, SurfaceSerializationFactory>()
            .AddSingleton<SerializationFactory, ChunkyImageSerializationFactory>()
            .AddSingleton<SerializationFactory, KernelSerializationFactory>()
            .AddSingleton<SerializationFactory, VecDSerializationFactory>()
            .AddSingleton<SerializationFactory, VecISerializationFactory>()
            .AddSingleton<SerializationFactory, ColorSerializationFactory>()
            .AddSingleton<SerializationFactory, ColorMatrixSerializationFactory>()
            .AddSingleton<SerializationFactory, VecD3SerializationFactory>()
            .AddSingleton<SerializationFactory, TextureSerializationFactory>()
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
            .AddMenuBuilders()
            .AddAnalyticsAsNeeded();
    }

    private static IServiceCollection AddAnalyticsAsNeeded(this IServiceCollection collection)
    {
        string url = BuildConstants.AnalyticsUrl;

        if (url == "${analytics-url}")
        {
            url = null;
            SetDebugUrl(ref url);
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            collection
                .AddSingleton<AnalyticsClient>(_ => new AnalyticsClient(url))
                .AddSingleton<AnalyticsPeriodicReporter>();
        }

        return collection;

        [Conditional("DEBUG")]
        static void SetDebugUrl(ref string? url)
        {
            url = Environment.GetEnvironmentVariable("PixiEditorAnalytics");
        }
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
