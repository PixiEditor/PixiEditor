using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Views.Main;

public partial class CreateDocumentFallbackView : UserControl
{
    public CreateDocumentFallbackView()
    {
        InitializeComponent();
    }

    private void CreateDocumentClick(object? sender, RoutedEventArgs e)
    {
        var fileViewModel = ViewModelMain.Current.FileSubViewModel;

        var width = sizePicker.ChosenWidth;
        var height = sizePicker.ChosenHeight;
        
        fileViewModel.NewDocument(b => b
            .WithSize(width, height)
            .WithLayer(l => l
                .WithName(new LocalizedString("BASE_LAYER_NAME"))
                .WithSurface(new Surface(new VecI(width, height)))));
    }
}

