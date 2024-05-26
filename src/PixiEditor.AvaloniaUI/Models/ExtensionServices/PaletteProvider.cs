using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.AvaloniaUI.Models.Palettes;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Platform;

namespace PixiEditor.AvaloniaUI.Models.ExtensionServices;

internal sealed class PaletteProvider : IPaletteProvider
{
    public ObservableCollection<PaletteFileParser> AvailableParsers { get; set; }
    public ObservableCollection<PaletteListDataSource> DataSources => dataSources;
    private readonly ObservableCollection<PaletteListDataSource> dataSources;

    public PaletteProvider()
    {
        dataSources = new ObservableCollection<PaletteListDataSource>();
    }

    public async AsyncCall<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering)
    {
        List<IPalette> allPalettes = new();
        foreach (PaletteListDataSource dataSource in dataSources)
        {
            var palettes = await dataSource.FetchPaletteList(startIndex, items, filtering);
            allPalettes.AddRange(palettes);
        }

        return allPalettes;
    }

    public async AsyncCall<bool> AddPalette(IPalette palette, bool overwrite = false)
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
