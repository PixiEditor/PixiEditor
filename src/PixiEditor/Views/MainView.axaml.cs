using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Interop.Avalonia.Core;
using Drawie.Interop.Avalonia.OpenGl;
using Drawie.Numerics;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Behaviors;
using PixiEditor.ViewModels;

namespace PixiEditor.Views;

public partial class MainView : UserControl
{
    private ViewModelMain Context => (ViewModelMain)DataContext;

    public MainView()
    {
        InitializeComponent();
        TextBoxFocusBehavior.FallbackFocusElement = FocusableGrid;
        DropGrid.AddHandler(DragDrop.DragEnterEvent, MainView_DragEnter);
        DropGrid.AddHandler(DragDrop.DragLeaveEvent, MainView_DragLeave);
        DropGrid.AddHandler(DragDrop.DropEvent, MainView_Drop);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // hacky way to fix first element not rendering
        // feel free to make a proper fix inside Drawie
        if (IDrawieInteropContext.Current is OpenGlInteropContext)
        {
            OpenGlInitDummy.IsVisible = true;
            OpenGlInitDummy.Texture = new Texture(new Texture(new VecI(1, 1)));
            OpenGlInitDummy.QueueNextFrame();
            Dispatcher.UIThread.Post(() =>
            {
                OpenGlInitDummy.Texture.Dispose();
                OpenGlInitDummy.Texture = null;
                OpenGlInitDummy.IsVisible = false;
            });
        }
        else
        {
            OpenGlInitDummy.IsVisible = false;
        }

        if (DataContext is ViewModelMain vm)
        {
            vm.OnStartup();
        }
    }

    private void MainView_Drop(object sender, DragEventArgs e)
    {
        Context.ActionDisplays[nameof(MainView_Drop)] = null;

        var fileDropList = e.Data.GetFileDropList();
        if (fileDropList == null || fileDropList.Length == 0)
        {
            if (!ColorHelper.ParseAnyFormat(e.Data, out var color))
            {
                return;
            }

            e.DragEffects = DragDropEffects.Copy;
            Context.ColorsSubViewModel.PrimaryColor = color.Value;
            return;
        }

        if (fileDropList is { Length: > 0 })
        {
            foreach (var item in fileDropList)
            {
                Importer.IsSupportedFile(item.Path.LocalPath);
                Context.FileSubViewModel.OpenFromPath(item.Path.LocalPath);
            }
        }
    }

    private void MainView_DragEnter(object sender, DragEventArgs e)
    {
        if (!ClipboardController.IsImage(e.Data))
        {
            if (ColorHelper.ParseAnyFormat(e.Data, out _))
            {
                Context.ActionDisplays[nameof(MainView_Drop)] = "PASTE_AS_PRIMARY_COLOR";
                e.DragEffects = DragDropEffects.Copy;
                return;
            }

            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        Context.ActionDisplays[nameof(MainView_Drop)] = "IMPORT_AS_NEW_FILE";
    }

    private void MainView_DragLeave(object sender, DragEventArgs e)
    {
        Context.ActionDisplays[nameof(MainView_Drop)] = null;
    }
}
