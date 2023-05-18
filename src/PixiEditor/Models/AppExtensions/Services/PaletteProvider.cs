using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;

namespace PixiEditor.Models.AppExtensions.Services;

internal sealed class PaletteProvider : IPaletteProvider
{
    private List<PaletteListDataSource> dataSources;

    public PaletteProvider(List<PaletteListDataSource> dataSources)
    {
        this.dataSources = dataSources;
    }

    public async Task<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering)
    {
        List<IPalette> allPalettes = new();
        foreach (PaletteListDataSource dataSource in dataSources)
        {
            var palettes = await dataSource.FetchPaletteList(startIndex, items, filtering);
            allPalettes.AddRange(palettes.Palettes);
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
            palette.Colors.Select(x => new Color(x.R, x.G, x.B)).ToArray());

        return true;
    }
}
