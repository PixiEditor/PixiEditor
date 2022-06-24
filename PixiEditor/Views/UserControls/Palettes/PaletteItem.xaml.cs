using System;
using PixiEditor.Models.DataHolders.Palettes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls.Palettes
{
    /// <summary>
    /// Interaction logic for LospecPaletteItem.xaml
    /// </summary>
    public partial class PaletteItem : UserControl
    {
        public Palette Palette
        {
            get { return (Palette)GetValue(PaletteProperty); }
            set { SetValue(PaletteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Palette.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteProperty =
            DependencyProperty.Register("Palette", typeof(Palette), typeof(PaletteItem));

        public ICommand ImportPaletteCommand
        {
            get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
            set { SetValue(ImportPaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImportPaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImportPaletteCommandProperty =
            DependencyProperty.Register("ImportPaletteCommand", typeof(ICommand), typeof(PaletteItem));

        public ICommand DeletePaletteCommand
        {
            get { return (ICommand)GetValue(DeletePaletteCommandProperty); }
            set { SetValue(DeletePaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeletePaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeletePaletteCommandProperty =
            DependencyProperty.Register("DeletePaletteCommand", typeof(ICommand), typeof(PaletteItem));

        public static readonly DependencyProperty ToggleFavouriteCommandProperty = DependencyProperty.Register(
            "ToggleFavouriteCommand", typeof(ICommand), typeof(PaletteItem), new PropertyMetadata(default(ICommand)));

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
}
