using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.Sdk.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.Sdk.Bridge;

internal static partial class Interop
{
    public static void RegisterBrushTool(byte[] pixiFileBytes, CustomToolConfig config)
    {
        using MemoryStream stream = new();
        Serializer.Serialize(stream, config);

        byte[] data = stream.ToArray();
        Native.register_brush_tool(InteropUtility.ByteArrayToIntPtr(pixiFileBytes), pixiFileBytes.Length, InteropUtility.ByteArrayToIntPtr(data), data.Length);
    }
}
