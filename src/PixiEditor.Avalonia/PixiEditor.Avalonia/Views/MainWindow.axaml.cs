using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Avalonia.ViewModels;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Models.Localization;
using PixiEditor.Platform;
using PixiEditor.Views;

namespace PixiEditor.Avalonia.Views;

internal partial class MainWindow : Window
{
    private readonly IPreferences preferences;
    private readonly IPlatform platform;
    private readonly IServiceProvider services;
    private static ExtensionLoader extLoader;

    public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }

    public MainWindow(ExtensionLoader extensionLoader)
    {
        extLoader = extensionLoader;

        services = new ServiceCollection()
            .AddPlatform()
            .AddPixiEditor(extensionLoader)
            .AddExtensionServices()
            .BuildServiceProvider();

        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend);

        preferences = services.GetRequiredService<IPreferences>();
        platform = services.GetRequiredService<IPlatform>();
        DataContext = services.GetRequiredService<ViewModelMain>();
        DataContext.Setup(services);

        InitializeComponent();
    }

    public static MainWindow CreateWithDocuments(IEnumerable<(string? originalPath, byte[] dotPixiBytes)> documents)
    {
        //TODO: Implement this
        /*MainWindow window = new(extLoader);
        FileViewModel fileVM = window.services.GetRequiredService<FileViewModel>();

        foreach (var (path, bytes) in documents)
        {
            fileVM.OpenRecoveredDotPixi(path, bytes);
        }

        return window;*/

        return new MainWindow(null);
    }
}
