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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.ExternalServices;
using PixiEditor.Models.Undo;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand<(SKColor, SKColor)> ReplaceColorsCommand { get; set; }

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
            ImportPaletteCommand = new RelayCommand<List<string>>(ImportPalette, Owner.DocumentIsNotNull);
            ReplaceColorsCommand = new RelayCommand<(SKColor oldColor, SKColor newColor)>(ReplaceColors, Owner.DocumentIsNotNull);
            Owner.OnStartupEvent += OwnerOnStartupEvent;
        }

        private void ReplaceColors((SKColor oldColor, SKColor newColor) colors)
        {
            Document activeDocument = Owner.BitmapManager?.ActiveDocument;
            if (activeDocument != null)
            {
                activeDocument.ReplaceColor(colors.oldColor, colors.newColor);
                ReplacePaletteColor(colors, activeDocument);
                activeDocument.UndoManager.AddUndoChange(new Change(
                    ReplacePaletteColorProcess,
                    new object[] { (colors.newColor, colors.oldColor), activeDocument },
                    ReplacePaletteColorProcess,
                    new object[] { colors, activeDocument }));
                activeDocument.UndoManager.SquashUndoChanges(2, $"Replace color {colors.oldColor} with {colors.newColor}");
            }

        }

        private static void ReplacePaletteColorProcess(object[] args)
        {
            (SKColor oldColor, SKColor newColor) colors = ((SKColor, SKColor))args[0];
            Document activeDocument = (Document)args[1];

            ReplacePaletteColor(colors, activeDocument);
        }

        private static void ReplacePaletteColor((SKColor oldColor, SKColor newColor) colors, Document activeDocument)
        {
            int oldIndex = activeDocument.Palette.IndexOf(colors.oldColor);
            if (oldIndex != -1)
            {
                activeDocument.Palette[oldIndex] = colors.newColor;
            }
        }

        private async void OwnerOnStartupEvent(object? sender, EventArgs e)
        {
            await ImportLospecPalette();
        }

        private async Task ImportLospecPalette()
        {
            var args = StartupArgs.Args;
            var lospecPaletteArg = args.FirstOrDefault(x => x.StartsWith("lospec-palette://"));

            if (lospecPaletteArg != null)
            {
                var browser = PalettesBrowser.Open(PaletteDataSources, ImportPaletteCommand,
                    new WpfObservableRangeCollection<SKColor>());

                browser.IsFetching = true;
                var palette = await LospecPaletteFetcher.FetchPalette(lospecPaletteArg.Split(@"://")[1].Replace("/", ""));
                if (palette != null)
                {
                    if (LocalPalettesFetcher.PaletteExists(palette.Name))
                    {
                        var consent = ConfirmationDialog.Show(
                            $"Palette '{palette.Name}' already exists, do you want to overwrite it?", "Palette exists");
                        if (consent == ConfirmationType.No)
                        {
                            palette.Name = LocalPalettesFetcher.GetNonExistingName(palette.Name);
                        }
                        else if (consent == ConfirmationType.Canceled)
                        {
                            browser.IsFetching = false;
                            return;
                        }
                    }

                    await SavePalette(palette, browser);
                }
                else
                {
                    await browser.UpdatePaletteList();
                }
            }
        }

        private static async Task SavePalette(Palette palette, PalettesBrowser browser)
        {
            palette.FileName = $"{palette.Name}.pal";

            await LocalPalettesFetcher.SavePalette(
                palette.FileName,
                palette.Colors.Select(SKColor.Parse).ToArray());

            await browser.UpdatePaletteList();
            if (browser.SortedResults.Any(x => x.FileName == palette.FileName))
            {
                int indexOfImported =
                    browser.SortedResults.IndexOf(browser.SortedResults.First(x => x.FileName == palette.FileName));
                browser.SortedResults.Move(indexOfImported, 0);
            }
            else
            {
                browser.SortedResults.Insert(0, palette);
            }
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
            (PrimaryColor, SecondaryColor) = (SecondaryColor, PrimaryColor);
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
