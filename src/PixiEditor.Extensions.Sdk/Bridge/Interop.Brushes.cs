using PixiEditor.Extensions.CommonApi.Brushes;
using PixiEditor.Extensions.CommonApi.Utilities;
using PixiEditor.Extensions.Sdk.Api.Brushes;
using PixiEditor.Extensions.Sdk.Utilities;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    private static Dictionary<int, IBrushDataSource> brushesDataSources = new();

    public static void RegisterDataSource(IBrushDataSource dataSource)
    {
        if (brushesDataSources.ContainsValue(dataSource))
        {
            throw new InvalidOperationException("Data source is already registered.");
        }

        ByteWriter writer = new();
        writer.WriteInt(dataSource.GetBrushes().Count);
        foreach (var brush in dataSource.GetBrushes())
        {
            writer.WriteInt(brush.Length);
            writer.WriteBytes(brush);
        }

        var array = writer.ToArray();
        IntPtr arr = InteropUtility.ByteArrayToIntPtr(array);
        int handle = Native.register_brush_data_source(dataSource.Name, arr, array.Length);
        brushesDataSources[handle] = dataSource;
    }
}
