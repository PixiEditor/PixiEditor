using PixiEditor.Extensions.CommonApi.Palettes;
using ProtoBuf;

namespace PixiEditor.Extensions.WasmRuntime.Api.Palettes;

internal class PalettesApi : ApiGroupHandler
{
    [ApiFunction("register_palette_data_source")]
    public int RegisterPaletteDataSource(string name)
    {
        ExtensionPalettesDataSource dataSource = new(name, Extension);
        Api.Palettes.RegisterDataSource(dataSource);
        return NativeObjectManager.AddObject(dataSource);
    }
    
    [ApiFunction("set_fetch_palette_result")]
    public void SetFetchPaletteResult(int asyncHandle, Span<byte> paletteBytes)
    {
        using MemoryStream stream = new();
        
        stream.Write(paletteBytes);
        stream.Seek(0, SeekOrigin.Begin);
        
        PaletteListResult listResult = Serializer.Deserialize<PaletteListResult>(stream);
        AsyncHandleManager.SetAsyncCallResult(asyncHandle, listResult);
    }
}
