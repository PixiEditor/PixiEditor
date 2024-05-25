using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;

namespace PixiEditor.Models.AppExtensions.Services;

internal sealed class PaletteProvider : IPaletteProvider
{
    public WpfObservableRangeCollection<PaletteFileParser> AvailableParsers { get; set; }
    public WpfObservableRangeCollection<PaletteListDataSource> DataSources => dataSources;
    private readonly WpfObservableRangeCollection<PaletteListDataSource> dataSources;

    public PaletteProvider()
    {
        dataSources = new WpfObservableRangeCollection<PaletteListDataSource>();
    }

    public async Task<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering)
    {
        List<IPalette> allPalettes = new();
        foreach (PaletteListDataSource dataSource in dataSources)
        {
            var palettes = await dataSource.FetchPaletteList(startIndex, items, filtering);
            allPalettes.AddRange(palettes);
        }

        return allPalettes;
    }

    public async Task<bool> AddPalette(IPalette palette, bool overwrite = false)
    {
        LocalPalettesFetcher localPalettesFetcher = dataSources.OfType<LocalPalettesFetcher>().FirstOrDefault();
        if(localPalettesFetcher == null)
        {
            return false;
        }

        if (LocalPalettesFetcher.PaletteExists(palette.Name))
        {
            if (overwrite)
            {
                await localPalettesFetcher.DeletePalette(palette.Name);
            }
            else
            {
                return false;
            }
        }


        string finalName = LocalPalettesFetcher.GetNonExistingName(palette.Name, true);

        await localPalettesFetcher.SavePalette(
            finalName,
            palette.Colors.ToArray());

        return true;
    }

    public void RegisterDataSource(PaletteListDataSource dataSource)
    {
        if(dataSources.Contains(dataSource)) return;

        dataSources.Add(dataSource);

        dataSource.AvailableParsers = AvailableParsers.ToList();
        dataSource.Initialize();
    }
}
