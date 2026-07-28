namespace PixiEditor.Extensions.WasmRuntime.Api.Brushes;


internal class BrushesApi : ApiGroupHandler
{
    [ApiFunction("register_brush_data_source")]
    public int RegisterBrushDataSource(string name, Span<byte> data)
    {
        ExtensionBrushesDataSource dataSource = new(name, data, Extension);
        Api.Brushes.RegisterBrushDataSource(dataSource);
        return NativeObjectManager.AddObject(dataSource);
    }
}
