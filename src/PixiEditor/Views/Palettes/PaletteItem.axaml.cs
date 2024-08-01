using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using PixiEditor.Models.Palettes;
using PixiEditor.Views.Input;

namespace PixiEditor.Views.Palettes;

[PseudoClasses(":favourite")]
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

    static PaletteItem()
    {
        PaletteProperty.Changed.Subscribe(OnPaletteChanged);
    }

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
    
    private static void OnPaletteChanged(AvaloniaPropertyChangedEventArgs<Palette> e)
    {
        PaletteItem paletteItem = (PaletteItem)e.Sender;
        if(e.OldValue.Value != null)
        {
            e.OldValue.Value.PropertyChanged -= paletteItem.Palette_PropertyChanged;
        }
        if(e.NewValue.Value != null)
        {
            e.NewValue.Value.PropertyChanged += paletteItem.Palette_PropertyChanged;
            paletteItem.PseudoClasses.Set(":favourite", e.NewValue.Value.IsFavourite);
        }
    }

    private void Palette_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Palette.IsFavourite))
        {
            PseudoClasses.Set(":favourite", Palette.IsFavourite);
        }
    }
}
