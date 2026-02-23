using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.AnimationRenderer.FFmpeg;
using PixiEditor.Extensions.Commands;
using PixiEditor.Extensions.CommonApi.Commands;
using PixiEditor.Extensions.CommonApi.IO;
using PixiEditor.Extensions.CommonApi.Logging;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Extensions.CommonApi.Ui;
using PixiEditor.Extensions.CommonApi.User;
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
using PixiEditor.Models.IO.CustomDocumentFormats;
using PixiEditor.Models.IO.PaletteParsers;
using PixiEditor.Models.IO.PaletteParsers.JascPalFile;
using PixiEditor.Models.Localization;
using PixiEditor.Models.Palettes;
using PixiEditor.Models.Preferences;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.UI.Common.Localization;
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
            .AddSingleton<AutosaveViewModel>()
            .AddSingleton<UserViewModel>()
            .AddSingleton<BrushesViewModel>()
            .AddSingleton<IColorsHandler, ColorsViewModel>(x => x.GetRequiredService<ColorsViewModel>())
            .AddSingleton<IWindowHandler, WindowViewModel>(x => x.GetRequiredService<WindowViewModel>())
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
            .AddTool<MoveViewportToolViewModel>()
            .AddTool<RotateViewportToolViewModel>()
            .AddTool<IMoveToolHandler, MoveToolViewModel>()
            .AddTool<IPenToolHandler, PenToolViewModel>()
            .AddTool<ISelectToolHandler, SelectToolViewModel>()
            .AddTool<IMagicWandToolHandler, MagicWandToolViewModel>()
            .AddTool<ILassoToolHandler, LassoToolViewModel>()
            .AddTool<IFloodFillToolHandler, FloodFillToolViewModel>()
            .AddTool<ILineToolHandler, RasterLineToolViewModel>()
            .AddTool<IRasterEllipseToolHandler, RasterEllipseToolViewModel>()
            .AddTool<IRasterRectangleToolHandler, RasterRectangleToolViewModel>()
            .AddTool<IEraserToolHandler, EraserToolViewModel>()
            .AddTool<IColorPickerHandler, ColorPickerToolViewModel>()
            .AddTool<IVectorEllipseToolHandler, VectorEllipseToolViewModel>()
            .AddTool<IVectorRectangleToolHandler, VectorRectangleToolViewModel>()
            .AddTool<IVectorLineToolHandler, VectorLineToolViewModel>()
            .AddTool<IVectorPathToolHandler, VectorPathToolViewModel>()
            .AddTool<ITextToolHandler, TextToolViewModel>()
            .AddTool<ZoomToolViewModel>()
            // File types
            .AddSingleton<IoFileType, PixiFileType>()
            .AddSingleton<IoFileType, PngFileType>()
            .AddSingleton<IoFileType, JpegFileType>()
            .AddSingleton<IoFileType, BmpFileType>()
            .AddSingleton<IoFileType, WebpFileType>()
            .AddSingleton<IoFileType, GifFileType>()
            .AddSingleton<IoFileType, Mp4FileType>()
            .AddSingleton<IoFileType, SvgFileType>()
            .AddSingleton<IoFileType, TtfFileType>()
            .AddSingleton<IoFileType, OtfFileType>()
            // Serialization Factories
            .AddSerializationFactories()
            // Custom document builders
            .AddSingleton<IDocumentBuilder, SvgDocumentBuilder>()
            .AddSingleton<IDocumentBuilder, FontDocumentBuilder>()
            .AddSingleton<IDocumentBuilder, AnimationDocumentBuilder>(x =>
                new AnimationDocumentBuilder(new FFMpegRenderer()))
            .AddSingleton<IPalettesProvider, PaletteProvider>()
            .AddSingleton<CommandProvider>()
            .AddSingleton<IDocumentProvider, DocumentProvider>()
            .AddSingleton<ICommandProvider, CommandProvider>(x => x.GetRequiredService<CommandProvider>())
            .AddSingleton<IIconLookupProvider, DynamicResourceIconLookupProvider>()
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
            .AddSingleton<PaletteListDataSource, LocalPalettesFetcher>()
            .AddMenuBuilders()
            .AddAnalyticsAsNeeded();
    }

    private static IServiceCollection AddAnalyticsAsNeeded(this IServiceCollection collection)
    {
        var url = AnalyticsClient.GetAnalyticsUrl();

        if (!string.IsNullOrWhiteSpace(url))
        {
            collection
                .AddSingleton<AnalyticsClient>(_ => new AnalyticsClient(url))
                .AddSingleton<AnalyticsPeriodicReporter>();
        }

        return collection;
    }

    public static IServiceCollection AddSerializationFactories(this IServiceCollection collection)
    {
        collection
            .AddTransient<SerializationFactory, BrushSerializationFactory>()
            .AddTransient<SerializationFactory, ChunkyImageSerializationFactory>()
            .AddTransient<SerializationFactory, ColorMatrixSerializationFactory>()
            .AddTransient<SerializationFactory, ColorSerializationFactory>()
            .AddTransient<SerializationFactory, DocumentSerializationFactory>()
            .AddTransient<SerializationFactory, EllipseSerializationFactory>()
            .AddTransient<SerializationFactory, FontFamilySerializationFactory>()
            .AddTransient<SerializationFactory, KernelSerializationFactory>()
            .AddTransient<SerializationFactory, LineSerializationFactory>()
            .AddTransient<SerializationFactory, Matrix3X3SerializationFactory>()
            .AddTransient<SerializationFactory,
                Models.Serialization.Factories.Paintables.ColorPaintableSerializationFactory>()
            .AddTransient<SerializationFactory,
                Models.Serialization.Factories.Paintables.LinearGradientSerializationFactory>()
            .AddTransient<SerializationFactory,
                Models.Serialization.Factories.Paintables.RadialGradientSerializationFactory>()
            .AddTransient<SerializationFactory,
                Models.Serialization.Factories.Paintables.SweepGradientSerializationFactory>()
            .AddTransient<SerializationFactory, Models.Serialization.Factories.Paintables.
                TexturePaintableSerializationFactory>()
            .AddTransient<SerializationFactory, PointsDataSerializationFactory>()
            .AddTransient<SerializationFactory, RectangleSerializationFactory>()
            .AddTransient<SerializationFactory, SurfaceSerializationFactory>()
            .AddTransient<SerializationFactory, TextSerializationFactory>()
            .AddTransient<SerializationFactory, TextureSerializationFactory>()
            .AddTransient<SerializationFactory, VecD3SerializationFactory>()
            .AddTransient<SerializationFactory, VecD4SerializationFactory>()
            .AddTransient<SerializationFactory, VecDSerializationFactory>()
            .AddTransient<SerializationFactory, VecISerializationFactory>()
            .AddTransient<SerializationFactory, VectorPathSerializationFactory>();

        return collection;
    }
    
    private static IServiceCollection AddTool<T, T1>(this IServiceCollection collection)
        where T : class, IToolHandler where T1 : class, T
    {
        return collection.AddSingleton<T, T1>()
            .AddSingleton<IToolHandler, T1>(x => (T1)x.GetRequiredService<T>());
    }
    
    private static IServiceCollection AddTool<T>(this IServiceCollection collection)
        where T : class, IToolHandler
    {
        return collection.AddSingleton<IToolHandler, T>();
    }

    private static IServiceCollection AddMenuBuilders(this IServiceCollection collection)
    {
        return collection
            .AddSingleton<MenuItemBuilder, RecentFilesMenuBuilder>()
            .AddSingleton<MenuItemBuilder, FileExitMenuBuilder>()
            .AddSingleton<MenuItemBuilder, SymmetryMenuBuilder>()
            .AddSingleton<MenuItemBuilder, OpenDockablesMenuBuilder>()
            .AddSingleton<MenuItemBuilder, ToggleGridLinesMenuBuilder>()
            .AddSingleton<MenuItemBuilder, ToggleSnappingMenuBuilder>()
            .AddSingleton<MenuItemBuilder, ToggleHighResPreviewMenuBuilder>();
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
            .AddSingleton<ICommandSupervisor, CommandSupervisor>()
            .AddSingleton<ILogger, ConsoleLogger>()
            .AddSingleton<IVisualTreeProvider, VisualTreeProvider>()
            .AddSingleton<IUserDataProvider, UserDataProvider>()
            .AddSingleton<IFileSystemProvider, FileSystemProvider>();
}
