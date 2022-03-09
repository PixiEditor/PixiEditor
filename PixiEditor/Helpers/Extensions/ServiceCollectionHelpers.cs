using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.JascPalFile;
using PixiEditor.Models.Services;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Helpers.Extensions
{
    public static class ServiceCollectionHelpers
    {
        /// <summary>
        /// Add's all the services required to fully run PixiEditor's MainWindow
        /// </summary>
        public static IServiceCollection AddPixiEditor(this IServiceCollection collection) => collection
                .AddSingleton<ViewModelMain>()
                .AddSingleton<IPreferences, PreferencesSettings>()
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
                .AddSingleton<ViewportViewModel>()
                .AddSingleton<ColorsViewModel>()
                .AddSingleton<DocumentViewModel>()
                .AddSingleton<MiscViewModel>()
                .AddSingleton(x => new DiscordViewModel(x.GetService<ViewModelMain>(), "764168193685979138"))
                .AddSingleton<DebugViewModel>()
                // Controllers
                .AddSingleton<ShortcutController>()
                .AddSingleton<BitmapManager>()
                // Tools
                .AddSingleton<Tool, MoveViewportTool>()
                .AddSingleton<Tool, MoveTool>()
                .AddSingleton<Tool, PenTool>()
                .AddSingleton<Tool, SelectTool>()
                .AddSingleton<Tool, MagicWandTool>()
                .AddSingleton<Tool, FloodFillTool>()
                .AddSingleton<Tool, LineTool>()
                .AddSingleton<Tool, CircleTool>()
                .AddSingleton<Tool, RectangleTool>()
                .AddSingleton<Tool, EraserTool>()
                .AddSingleton<Tool, ColorPickerTool>()
                .AddSingleton<Tool, BrightnessTool>()
                .AddSingleton<Tool, ZoomTool>()
                // Palette Parsers
                .AddSingleton<PaletteFileParser, JascFileParser>()
                // Palette data sources
                .AddSingleton<PaletteListDataSource, LocalPalettesFetcher>()
                // Other
                .AddSingleton<DocumentProvider>();
    }
}
