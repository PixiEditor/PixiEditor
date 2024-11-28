using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Models.Palettes;
using PixiEditor.Platform;

namespace PixiEditor.Models.ExtensionServices;

internal sealed class PaletteProvider : IPalettesProvider
{
    public ObservableCollection<PaletteFileParser> AvailableParsers { get; set; }
    public ObservableCollection<PaletteListDataSource> DataSources => dataSources;
    private readonly ObservableCollection<PaletteListDataSource> dataSources;

    public PaletteProvider()
    {
        dataSources = new ObservableCollection<PaletteListDataSource>();
    }

    /// <summary>
    ///     Fetches palettes from the provider.
    /// </summary>
    /// <param name="startIndex">Starting fetch index. Palettes before said index won't be fetched.</param>
    /// <param name="items">Max amount of palettes to fetch.</param>
    /// <param name="filtering">Filtering settings for fetching.</param>
    /// <returns>List of palettes.</returns>
    public async AsyncCall<List<IPalette>> FetchPalettes(int startIndex, int items, FilteringSettings filtering)
    {
        List<IPalette> allPalettes = new();
        foreach (PaletteListDataSource dataSource in dataSources)
        {
            try
            {
                var palettes = await dataSource.FetchPaletteList(startIndex, items, filtering);
                allPalettes.AddRange(palettes);
            }
            catch
            {
#if DEBUG
                throw;
#endif
            }
        }

        return allPalettes;
    }

    /// <summary>
    ///     Adds a palette to the provider. This means that the palette will be saved in local storage.
    /// </summary>
    /// <param name="palette">Palette to save.</param>
    /// <param name="overwrite">If true and palette with the same name exists, it will be overwritten. If false and palette with the same name exists, it will not be added.</param>
    /// <returns>True if adding palette was successful.</returns>
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
