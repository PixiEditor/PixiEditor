using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Search;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ExternalServices;
using PixiEditor.Models.IO;
using PixiEditor.Models.Undo;
using PixiEditor.Views.Dialogs;
using SkiaSharp;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Colors", "Palette Colors")]
    public class ColorsViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand<List<string>> ImportPaletteCommand { get; set; }

        public WpfObservableRangeCollection<PaletteFileParser> PaletteParsers { get; private set; }
        public WpfObservableRangeCollection<PaletteListDataSource> PaletteDataSources { get; private set; }

        public LocalPalettesFetcher LocalPaletteFetcher => _localPaletteFetcher ??=
            (LocalPalettesFetcher)PaletteDataSources.FirstOrDefault(x => x is LocalPalettesFetcher)!;

        private SKColor primaryColor = SKColors.Black;
        private LocalPalettesFetcher _localPaletteFetcher;

        public SKColor PrimaryColor // Primary color, hooked with left mouse button
        {
            get => primaryColor;
            set
            {
                if (primaryColor != value)
                {
                    primaryColor = value;
                    Owner.BitmapManager.PrimaryColor = value;
                    RaisePropertyChanged(nameof(PrimaryColor));
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
                    RaisePropertyChanged(nameof(SecondaryColor));
                }
            }
        }

        public ColorsViewModel(ViewModelMain owner)
            : base(owner)
        {
            ImportPaletteCommand = new RelayCommand<List<string>>(ImportPalette, CanImportPalette);
            Owner.OnStartupEvent += OwnerOnStartupEvent;
        }

        [Evaluator.CanExecute("PixiEditor.Colors.CanReplaceColors")]
        public bool CanReplaceColors()
        {
            return ViewModelMain.Current?.BitmapManager?.ActiveDocument is not null;
        }

        [Command.Internal("PixiEditor.Colors.ReplaceColors")]
        public void ReplaceColors((SKColor oldColor, SKColor newColor) colors)
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

        private async void OwnerOnStartupEvent(object sender, EventArgs e)
        {
            await ImportLospecPalette();
        }

        [Command.Basic("PixiEditor.Colors.OpenPaletteBrowser", "Open Palette Browser", "Open Palette Browser", CanExecute = "PixiEditor.HasDocument", IconPath = "Globe.png")]
        public void OpenPalettesBrowser() => PalettesBrowser.Open(PaletteDataSources, ImportPaletteCommand, Owner.BitmapManager.ActiveDocument.Palette);
        
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

        private async Task SavePalette(Palette palette, PalettesBrowser browser)
        {
            palette.FileName = $"{palette.Name}.pal";

            await LocalPaletteFetcher.SavePalette(
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

        [Evaluator.CanExecute("PixiEditor.Colors.CanImportPalette")]
        public bool CanImportPalette(List<string> paletteColors)
        {
            return paletteColors is not null && Owner.DocumentIsNotNull(paletteColors) && paletteColors.Count > 0;
        }

        [Command.Internal("PixiEditor.Colors.ImportPalette", CanExecute = "PixiEditor.Colors.CanImportPalette")]
        public void ImportPalette(List<string> palette)
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            if (doc is null)
                return;

            if (ConfirmationDialog.Show("Replace current palette with selected one?", "Replace current palette") == ConfirmationType.Yes)
            {
                if (doc.Palette is null)
                {
                    doc.Palette = new WpfObservableRangeCollection<SKColor>();
                }

                doc.Palette.ReplaceRange(palette.Select(x => SKColor.Parse(x)));
            }
        }

        [Evaluator.CanExecute("PixiEditor.Colors.CanSelectPaletteColor")]
        public bool CanSelectPaletteColor(int index)
        {
            var document = Owner.BitmapManager.ActiveDocument;
            return document?.Palette is not null && document.Palette.Count > index;
        }

        [Evaluator.Icon("PixiEditor.Colors.FirstPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon1() => GetPaletteColorIcon(0);
        [Evaluator.Icon("PixiEditor.Colors.SecondPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon2() => GetPaletteColorIcon(1);
        [Evaluator.Icon("PixiEditor.Colors.ThirdPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon3() => GetPaletteColorIcon(2);
        [Evaluator.Icon("PixiEditor.Colors.FourthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon4() => GetPaletteColorIcon(3);
        [Evaluator.Icon("PixiEditor.Colors.FifthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon5() => GetPaletteColorIcon(4);
        [Evaluator.Icon("PixiEditor.Colors.SixthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon6() => GetPaletteColorIcon(5);
        [Evaluator.Icon("PixiEditor.Colors.SeventhPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon7() => GetPaletteColorIcon(6);
        [Evaluator.Icon("PixiEditor.Colors.EighthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon8() => GetPaletteColorIcon(7);
        [Evaluator.Icon("PixiEditor.Colors.NinthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon9() => GetPaletteColorIcon(8);
        [Evaluator.Icon("PixiEditor.Colors.TenthPaletteColorIcon")]
        public ImageSource GetPaletteColorIcon10() => GetPaletteColorIcon(9);


        private ImageSource GetPaletteColorIcon(int index)
        {
            var document = Owner.BitmapManager.ActiveDocument;

            SKColor color;
            if (document?.Palette is null || document.Palette.Count <= index)
                color = SKColors.Gray;
            else
                color = document.Palette[index];

            return ColorSearchResult.GetIcon(color);
        }

        [Command.Basic("PixiEditor.Colors.SelectFirstPaletteColor", "Select color 1", "Select the first color in the palette", Key = Key.D1, Parameter = 0, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FirstPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectSecondPaletteColor", "Select color 2", "Select the second color in the palette", Key = Key.D2, Parameter = 1, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SecondPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectThirdPaletteColor", "Select color 3", "Select the third color in the palette", Key = Key.D3, Parameter = 2, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.ThirdPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectFourthPaletteColor", "Select color 4", "Select the fourth color in the palette", Key = Key.D4, Parameter = 3, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FourthPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectFifthPaletteColor", "Select color 5", "Select the fifth color in the palette", Key = Key.D5, Parameter = 4, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FifthPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectSixthPaletteColor", "Select color 6", "Select the sixth color in the palette", Key = Key.D6, Parameter = 5, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SixthPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectSeventhPaletteColor", "Select color 7", "Select the seventh color in the palette", Key = Key.D7, Parameter = 6, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SeventhPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectEighthPaletteColor", "Select color 8", "Select the eighth color in the palette", Key = Key.D8, Parameter = 7, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.EighthPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectNinthPaletteColor", "Select color 9", "Select the ninth color in the palette", Key = Key.D9, Parameter = 8, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.NinthPaletteColorIcon")]
        [Command.Basic("PixiEditor.Colors.SelectTenthPaletteColor", "Select color 10", "Select the tenth color in the palette", Key = Key.D0, Parameter = 9, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.TenthPaletteColorIcon")]
        public void SelectPaletteColor(int index)
        {
            var document = Owner.BitmapManager.ActiveDocument;
            if (document.Palette != null && document.Palette.Count > index)
            {
                PrimaryColor = document.Palette[index];
            }
        }

        [Command.Basic("PixiEditor.Colors.Swap", "Swap colors", "Swap primary and secondary colors", Key = Key.X)]
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

        [Command.Internal("PixiEditor.Colors.RemoveSwatch")]
        public void RemoveSwatch(SKColor color)
        {
            if (Owner.BitmapManager.ActiveDocument.Swatches.Contains(color))
            {
                Owner.BitmapManager.ActiveDocument.Swatches.Remove(color);
            }
        }

        [Command.Internal("PixiEditor.Colors.SelectColor")]
        public void SelectColor(SKColor color)
        {
            PrimaryColor = color;
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
