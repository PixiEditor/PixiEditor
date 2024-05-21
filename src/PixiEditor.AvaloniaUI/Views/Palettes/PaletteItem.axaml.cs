using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PixiEditor.AvaloniaUI.Models.Palettes;
using PixiEditor.AvaloniaUI.Views.Input;

namespace PixiEditor.AvaloniaUI.Views.Palettes;

internal partial class PaletteItem : UserControl
{
    public Palette Palette
    {
        get { return (Palette)GetValue(PaletteProperty); }
        set { SetValue(PaletteProperty, value); }
    }

    public static readonly StyledProperty<Palette> PaletteProperty =
        AvaloniaProperty.Register<PaletteItem, Palette>(
            nameof(Palette));

    public ICommand ImportPaletteCommand
    {
        get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
        set { SetValue(ImportPaletteCommandProperty, value); }
    }

    public static readonly StyledProperty<ICommand> ImportPaletteCommandProperty =
        AvaloniaProperty.Register<PaletteItem, ICommand>(
            nameof(ImportPaletteCommand));

    public ICommand DeletePaletteCommand
    {
        get { return (ICommand)GetValue(DeletePaletteCommandProperty); }
        set { SetValue(DeletePaletteCommandProperty, value); }
    }

    public static readonly StyledProperty<ICommand> DeletePaletteCommandProperty =
        AvaloniaProperty.Register<PaletteItem, ICommand>(
            nameof(DeletePaletteCommand));

    public static readonly StyledProperty<ICommand> ToggleFavouriteCommandProperty =
        AvaloniaProperty.Register<PaletteItem, ICommand>(
        nameof(ToggleFavouriteCommand));

    public ICommand ToggleFavouriteCommand
    {
        get { return (ICommand)GetValue(ToggleFavouriteCommandProperty); }
        set { SetValue(ToggleFavouriteCommandProperty, value); }
    }

    public event EventHandler<EditableTextBlock.TextChangedEventArgs> OnRename;


    public PaletteItem()
    {
        InitializeComponent();
    }

    private void EditableTextBlock_OnSubmit(object sender, EditableTextBlock.TextChangedEventArgs e)
    {
        OnRename?.Invoke(this, e);
    }

    private void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        titleTextBlock.IsEditing = true;
    }
}
