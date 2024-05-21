using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;

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
        //TODO: Implement
        /*LocalPalettesFetcher localPalettesFetcher = dataSources.OfType<LocalPalettesFetcher>().FirstOrDefault();
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
            palette.Colors.ToArray());*/

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
