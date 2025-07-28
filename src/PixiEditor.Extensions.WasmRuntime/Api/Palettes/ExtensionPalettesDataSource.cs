using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Palettes;
using ProtoBuf;

namespace PixiEditor.Extensions.WasmRuntime.Api.Palettes;

public class ExtensionPalettesDataSource : PaletteListDataSource
{
    private WasmExtensionInstance extension;
    
    public ExtensionPalettesDataSource(string name, WasmExtensionInstance instance) : base(name)
    {
        extension = instance;
    }

    public override async AsyncCall<List<IPalette>> FetchPaletteList(int startIndex, int items, FilteringSettings filtering)
    {
        PaletteListResult result = await extension.AsyncHandleManager.InvokeAsync<PaletteListResult>(
            (int asyncHandle) =>
            {
                var action = extension.Instance.GetAction<int, int, int>("fetch_palette_list");
                
                using MemoryStream stream = new();
                Serializer.Serialize(stream, new FetchPaletteListQuery(Name, startIndex, items, filtering));
                byte[] bytes = stream.ToArray();
                int ptr = extension.WasmMemoryUtility.WriteBytes(bytes);
                
                action.Invoke(ptr, bytes.Length, asyncHandle);
                extension.WasmMemoryUtility.Free(ptr);
            });

        foreach (var palette in result.Palettes)
        {
            palette.Source = this;
        }
        
        return await AsyncCall<List<IPalette>>.FromResult(result.Palettes.Cast<IPalette>().ToList());
    }
}
