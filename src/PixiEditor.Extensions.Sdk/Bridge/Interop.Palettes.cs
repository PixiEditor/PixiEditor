using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    private static Dictionary<int, PaletteListDataSource> dataSources = new();
    public static void RegisterDataSource(PaletteListDataSource dataSource)
    {
        if (dataSources.ContainsValue(dataSource))
        {
            throw new InvalidOperationException("Data source is already registered.");
        }

        int handle = Native.register_palette_data_source(dataSource.Name);
        dataSources[handle] = dataSource;
    }

    public static void FetchPaletteList(FetchPaletteListQuery query, int asyncHandle)
    {
        foreach (var source in dataSources)
        {
            if(source.Value.Name == query.DataSourceName)
            {
                source.Value.FetchPaletteList(query.StartIndex, query.Items, query.Filtering).Completed += (result) =>
                {
                    List<ExtensionPalette> palettes = new List<ExtensionPalette>();
                    foreach (var palette in result)
                    {
                        if (palette is ExtensionPalette extensionPalette)
                        {
                            palettes.Add(extensionPalette);
                        }
                        else
                        {
                            palettes.Add(new ExtensionPalette(palette.Name, palette.Colors, source.Value));
                        }
                    }
                    SetFetchPaletteResult(asyncHandle, new PaletteListResult(palettes.ToArray()));
                };
                return;
            }
        }
    }
    
    public static void SetFetchPaletteResult(int asyncHandle, PaletteListResult result)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, result);
        byte[] bytes = stream.ToArray();
        Native.set_fetch_palette_result(asyncHandle, InteropUtility.ByteArrayToIntPtr(bytes), bytes.Length);
    }
}
