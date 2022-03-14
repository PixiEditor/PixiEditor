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
            DependencyProperty.Register("Palette", typeof(Palette), typeof(PaletteItem), new PropertyMetadata(null));

        public ICommand ImportPaletteCommand
        {
            get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
            set { SetValue(ImportPaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImportPaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImportPaletteCommandProperty =
            DependencyProperty.Register("ImportPaletteCommand", typeof(ICommand), typeof(PaletteItem));


        public event EventHandler<EditableTextBlock.TextChangedEventArgs> OnRename;

        public PaletteItem()
        {
            InitializeComponent();
        }

        private void EditableTextBlock_OnSubmit(object sender, EditableTextBlock.TextChangedEventArgs e)
        {
            OnRename?.Invoke(this, e);
        }
    }
}
