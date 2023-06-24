using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Localization;
using PixiEditor.Models.Commands.XAML;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.ExternalServices;
using PixiEditor.Models.IO;
using PixiEditor.Views.Dialogs;
using Color = PixiEditor.DrawingApi.Core.ColorsImpl.Color;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;
using Command = PixiEditor.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Colors", "PALETTE_COLORS")]
internal class ColorsViewModel : SubViewModel<ViewModelMain>
{
    public RelayCommand<List<string>> ImportPaletteCommand { get; set; }

    public WpfObservableRangeCollection<PaletteFileParser> PaletteParsers { get; private set; }
    public WpfObservableRangeCollection<PaletteListDataSource> PaletteDataSources { get; private set; }

    public LocalPalettesFetcher LocalPaletteFetcher => _localPaletteFetcher ??=
        (LocalPalettesFetcher)PaletteDataSources.FirstOrDefault(x => x is LocalPalettesFetcher)!;

    private Color primaryColor = Colors.Black;
    private LocalPalettesFetcher _localPaletteFetcher;

    public Color PrimaryColor // Primary color, hooked with left mouse button
    {
        get => primaryColor;
        set
        {
            if (primaryColor != value)
            {
                primaryColor = value;
                RaisePropertyChanged(nameof(PrimaryColor));
            }
        }
    }

    private Color secondaryColor = Colors.White;

    public Color SecondaryColor
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
        return ViewModelMain.Current?.DocumentManagerSubViewModel?.ActiveDocument is not null;
    }

    [Command.Internal("PixiEditor.Colors.ReplaceColors")]
    public void ReplaceColors((Color oldColor, Color newColor) colors)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null || colors.oldColor == colors.newColor)
            return;
        doc.Operations.ReplaceColor(colors.oldColor, colors.newColor);
    }

    [Command.Basic("PixiEditor.Colors.ReplaceSecondaryByPrimaryColor", false, "REPLACE_SECONDARY_BY_PRIMARY", "REPLACE_SECONDARY_BY_PRIMARY", IconEvaluator = "PixiEditor.Colors.ReplaceColorIcon")]
    [Command.Basic("PixiEditor.Colors.ReplacePrimaryBySecondaryColor", true, "REPLACE_PRIMARY_BY_SECONDARY", "REPLACE_PRIMARY_BY_SECONDARY_DESCRIPTIVE", IconEvaluator = "PixiEditor.Colors.ReplaceColorIcon")]
    public void ReplaceColors(bool replacePrimary)
    {
        var oldColor = replacePrimary ? PrimaryColor : SecondaryColor;
        var newColor = replacePrimary ? SecondaryColor : PrimaryColor;
        
        ReplaceColors((oldColor, newColor));
    }

    [Evaluator.Icon("PixiEditor.Colors.ReplaceColorIcon")]
    public ImageSource ReplaceColorsIcon(object command)
    {
        bool replacePrimary = command switch
        {
            CommandSearchResult result => (bool)result.Command.GetParameter(),
            Models.Commands.Commands.Command cmd => (bool)cmd.GetParameter(),
            _ => false
        };
        
        var oldColor = replacePrimary ? PrimaryColor : SecondaryColor;
        var newColor = replacePrimary ? SecondaryColor : PrimaryColor;
        
        var oldDrawing = new GeometryDrawing { Brush = new SolidColorBrush(oldColor.ToOpaqueMediaColor()), Pen = new(Brushes.Gray, .5) };
        var oldGeometry = new EllipseGeometry(new Point(5, 5), 5, 5);
        
        oldDrawing.Geometry = oldGeometry;
        
        var newDrawing = new GeometryDrawing { Brush = new SolidColorBrush(newColor.ToOpaqueMediaColor()), Pen = new(Brushes.White, 1) };
        var newGeometry = new EllipseGeometry(new Point(10, 10), 6, 6);

        newDrawing.Geometry = newGeometry;
        
        return new DrawingImage(new DrawingGroup
        {
            Children = new DrawingCollection
            {
                oldDrawing,
                newDrawing
            }
        });
    }

    private async void OwnerOnStartupEvent(object sender, EventArgs e)
    {
        await ImportLospecPalette();
    }

    [Command.Basic("PixiEditor.Colors.OpenPaletteBrowser", "OPEN_PALETTE_BROWSER", "OPEN_PALETTE_BROWSER", CanExecute = "PixiEditor.HasDocument", IconPath = "Globe.png")]
    public void OpenPalettesBrowser() 
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is not null)
            PalettesBrowser.Open(PaletteDataSources, ImportPaletteCommand, doc.Palette);
    } 

    private async Task ImportLospecPalette()
    {
        var args = StartupArgs.Args;
        var lospecPaletteArg = args.FirstOrDefault(x => x.StartsWith("lospec-palette://"));

        if (lospecPaletteArg != null)
        {
            var browser = PalettesBrowser.Open(PaletteDataSources, ImportPaletteCommand,
                new WpfObservableRangeCollection<Color>());

            browser.IsFetching = true;
            var palette = await LospecPaletteFetcher.FetchPalette(lospecPaletteArg.Split(@"://")[1].Replace("/", ""));
            if (palette != null)
            {
                if (LocalPalettesFetcher.PaletteExists(palette.Name))
                {
                    var consent = ConfirmationDialog.Show(
                        new LocalizedString("OVERWRITE_PALETTE_CONSENT", palette.Name),
                        new LocalizedString("PALETTE_EXISTS"));
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
            palette.Colors.Select(Color.Parse).ToArray());

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
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        if (ConfirmationDialog.Show(new LocalizedString("REPLACE_PALETTE_CONSENT"), new LocalizedString("REPLACE_PALETTE")) == ConfirmationType.Yes)
        {
            if (doc.Palette is null)
            {
                doc.Palette = new WpfObservableRangeCollection<DrawingApi.Core.ColorsImpl.Color>();
            }

            doc.Palette.ReplaceRange(palette.Select(Color.Parse));
        }
    }

    [Evaluator.CanExecute("PixiEditor.Colors.CanSelectPaletteColor")]
    public bool CanSelectPaletteColor(int index)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
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
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;

        Color color;
        if (document?.Palette is null || document.Palette.Count <= index)
            color = Colors.Gray;
        else
            color = document.Palette[index];

        return ColorSearchResult.GetIcon(color);
    }

    [Command.Basic("PixiEditor.Colors.SelectFirstPaletteColor", "SELECT_COLOR_1", "SELECT_COLOR_1_DESCRIPTIVE", Key = Key.D1, Parameter = 0, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FirstPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectSecondPaletteColor", "SELECT_COLOR_2", "SELECT_COLOR_2_DESCRIPTIVE", Key = Key.D2, Parameter = 1, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SecondPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectThirdPaletteColor", "SELECT_COLOR_3", "SELECT_COLOR_3_DESCRIPTIVE", Key = Key.D3, Parameter = 2, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.ThirdPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectFourthPaletteColor", "SELECT_COLOR_4", "SELECT_COLOR_4_DESCRIPTIVE", Key = Key.D4, Parameter = 3, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FourthPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectFifthPaletteColor", "SELECT_COLOR_5", "SELECT_COLOR_5_DESCRIPTIVE", Key = Key.D5, Parameter = 4, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.FifthPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectSixthPaletteColor", "SELECT_COLOR_6", "SELECT_COLOR_6_DESCRIPTIVE", Key = Key.D6, Parameter = 5, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SixthPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectSeventhPaletteColor", "SELECT_COLOR_7", "SELECT_COLOR_7_DESCRIPTIVE", Key = Key.D7, Parameter = 6, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.SeventhPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectEighthPaletteColor", "SELECT_COLOR_8", "SELECT_COLOR_8_DESCRIPTIVE", Key = Key.D8, Parameter = 7, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.EighthPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectNinthPaletteColor", "SELECT_COLOR_9", "SELECT_COLOR_9_DESCRIPTIVE", Key = Key.D9, Parameter = 8, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.NinthPaletteColorIcon")]
    [Command.Basic("PixiEditor.Colors.SelectTenthPaletteColor", "SELECT_COLOR_10", "SELECT_COLOR_10_DESCRIPTIVE", Key = Key.D0, Parameter = 9, CanExecute = "PixiEditor.Colors.CanSelectPaletteColor", IconEvaluator = "PixiEditor.Colors.TenthPaletteColorIcon")]
    public void SelectPaletteColor(int index)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (document?.Palette is not null && document.Palette.Count > index)
        {
            PrimaryColor = document.Palette[index];
        }
    }

    [Command.Basic("PixiEditor.Colors.Swap", "SWAP_COLORS", "SWAP_COLORS_DESCRIPTIVE", Key = Key.X)]
    public void SwapColors(object parameter)
    {
        (PrimaryColor, SecondaryColor) = (SecondaryColor, PrimaryColor);
    }

    public void AddSwatch(Color color)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        if (!doc.Swatches.Contains(color))
        {
            doc.Swatches.Add(color);
        }
    }

    [Command.Internal("PixiEditor.Colors.RemoveSwatch")]
    public void RemoveSwatch(Color color)
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        if (doc.Swatches.Contains(color))
        {
            doc.Swatches.Remove(color);
        }
    }

    [Command.Internal("PixiEditor.Colors.SelectColor")]
    public void SelectColor(Color color)
    {
        PrimaryColor = color;
    }

    [Command.Basic("PixIEditor.Colors.AddPrimaryToPalettes", "ADD_PRIMARY_COLOR_TO_PALETTE", "ADD_PRIMARY_COLOR_TO_PALETTE_DESCRIPTIVE", CanExecute = "PixiEditor.HasDocument", IconPath = "CopyAdd.png")]
    public void AddPrimaryColorToPalette()
    {
        var palette = Owner.DocumentManagerSubViewModel.ActiveDocument.Palette;

        if (!palette.Contains(PrimaryColor))
        {
            palette.Add(PrimaryColor);
        }
    }

    [Command.Internal("PixiEditor.CloseContextMenu")]
    public void CloseContextMenu(System.Windows.Controls.ContextMenu menu)
    {
        menu.IsOpen = false;
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
