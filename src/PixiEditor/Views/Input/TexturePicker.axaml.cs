using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Models.IO;
using Brush = PixiEditor.Models.BrushEngine.Brush;

namespace PixiEditor.Views.Input;

internal partial class TexturePicker : UserControl
{
    public static readonly StyledProperty<Texture?> TextureProperty = AvaloniaProperty.Register<TexturePicker, Texture?>(
        nameof(Texture));

    public Texture? Texture
    {
        get => GetValue(TextureProperty);
        set => SetValue(TextureProperty, value);
    }

    public ICommand PickCommand { get; }
    public RelayCommand RemoveTextureCommand => new RelayCommand(() => Texture = null);

    public TexturePicker()
    {
        InitializeComponent();
        PickCommand = new AsyncRelayCommand(PickTextureFile);
    }

    private async Task PickTextureFile()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Image).GetFormattedTypes(false);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = any.ToList() });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            Texture?.Dispose();
            Texture = Texture.Load(dialog[0].Path.LocalPath);
        }
    }
}
