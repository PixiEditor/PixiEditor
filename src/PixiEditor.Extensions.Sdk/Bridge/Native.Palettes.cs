using System.Runtime.CompilerServices;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Native
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern int register_palette_data_source(string dataSourceName);

    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern void set_fetch_palette_result(int asyncHandle, IntPtr ptr, int length);

    [ApiExport("fetch_palette_list")]
    public static void FetchPaletteList(IntPtr queryPtr, int length, int asyncHandle)
    {
        byte[] bytes = InteropUtility.IntPtrToByteArray(queryPtr, length);
        using MemoryStream stream = new(bytes);
        FetchPaletteListQuery query = Serializer.Deserialize<FetchPaletteListQuery>(stream);
        Interop.FetchPaletteList(query, asyncHandle);
    }
}

