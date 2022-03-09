using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand SwapColorsCommand { get; set; }

        public RelayCommand SelectColorCommand { get; set; }

        public RelayCommand RemoveSwatchCommand { get; set; }

        public RelayCommand<List<string>> ImportPaletteCommand { get; set; }

        public RelayCommand<int> SelectPaletteColorCommand { get; set; }

        public WpfObservableRangeCollection<PaletteFileParser> PaletteParsers { get; private set; }
        public WpfObservableRangeCollection<PaletteListDataSource> PaletteDataSources { get; private set; }

        private SKColor primaryColor = SKColors.Black;

        public SKColor PrimaryColor // Primary color, hooked with left mouse button
        {
            get => primaryColor;
            set
            {
                if (primaryColor != value)
                {
                    primaryColor = value;
                    Owner.BitmapManager.PrimaryColor = value;
                    RaisePropertyChanged("PrimaryColor");
                }
            }
        }

        private SKColor secondaryColor = SKColors.White;

        public SKColor SecondaryColor
        {
            get => secondaryColor;
            set
            {
                if (secondaryColor != value)
                {
                    secondaryColor = value;
                    RaisePropertyChanged("SecondaryColor");
                }
            }
        }

        public ColorsViewModel(ViewModelMain owner)
            : base(owner)
        {

            SelectColorCommand = new RelayCommand(SelectColor);
            RemoveSwatchCommand = new RelayCommand(RemoveSwatch);
            SwapColorsCommand = new RelayCommand(SwapColors);
            SelectPaletteColorCommand = new RelayCommand<int>(SelectPaletteColor);
            ImportPaletteCommand = new RelayCommand<List<string>>(ImportPalette);
        }

        public void ImportPalette(List<string> palette)
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            if (doc == null) return;

            if (ConfirmationDialog.Show("Replace current palette with selected one?", "Replace current palette") == ConfirmationType.Yes)
            {
                if (doc.Palette == null)
                {
                    doc.Palette = new WpfObservableRangeCollection<SKColor>();
                }

                doc.Palette.ReplaceRange(palette.Select(x => SKColor.Parse(x)));
            }
        }

        private void SelectPaletteColor(int index)
        {
            var document = Owner.BitmapManager.ActiveDocument;
            if(document.Palette != null && document.Palette.Count > index)
            {
                PrimaryColor = document.Palette[index];
            }
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }

        public void AddSwatch(SKColor color)
        {
            if (!Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Add(color);
            }
        }

        private void RemoveSwatch(object parameter)
        {
            if (!(parameter is SKColor))
            {
                throw new ArgumentException();
            }

            SKColor color = (SKColor)parameter;
            if (Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Remove(color);
            }
        }

        private void SelectColor(object parameter)
        {
            PrimaryColor = parameter as SKColor? ?? throw new ArgumentException();
        }

        public void SetupPaletteParsers(IServiceProvider services)
        {
            PaletteParsers = new WpfObservableRangeCollection<PaletteFileParser>(services.GetServices<PaletteFileParser>());
            PaletteDataSources = new WpfObservableRangeCollection<PaletteListDataSource>(services.GetServices<PaletteListDataSource>());
            var parsers = PaletteParsers.ToList();

            foreach (var dataSource in PaletteDataSources)
            {
                dataSource.AvailableParsers = parsers;
                dataSource.Initialize();
            }
        }
    }
}
