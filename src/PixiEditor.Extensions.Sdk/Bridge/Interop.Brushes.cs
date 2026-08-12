using System.Runtime.InteropServices;
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
        var brushes = dataSource.GetBrushes();
        writer.WriteInt(brushes.Count);
        foreach (var brush in brushes)
        {
            writer.WriteInt(brush.Length);
            writer.WriteBytes(brush);
        }

        var array = writer.ToArray();
        IntPtr arr = InteropUtility.ByteArrayToIntPtr(array);
        int handle = Native.register_brush_data_source(dataSource.Name, arr, array.Length);
        brushesDataSources[handle] = dataSource;
        Marshal.FreeHGlobal(arr);
    }
}
