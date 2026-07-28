using PixiEditor.Extensions.CommonApi.Brushes;
using PixiEditor.Extensions.CommonApi.Utilities;

namespace PixiEditor.Extensions.WasmRuntime.Api.Brushes;

public class ExtensionBrushesDataSource : IBrushDataSource
{
    public string Name { get; }
    public Extension Owner { get; }
    private List<byte[]> brushes = new List<byte[]>();

    public ExtensionBrushesDataSource(string name, Span<byte> brushes, Extension extension)
    {
        Name = name;
        Owner = extension;
        LoadFromByteArray(brushes);
    }

    private void LoadFromByteArray(Span<byte> span)
    {
        ByteReader reader = new ByteReader(span.ToArray());
        int brushes = reader.ReadInt();
        for (int i = 0; i < brushes; i++)
        {
            int length = reader.ReadInt();
            byte[] brush = reader.ReadBytes(length);
            this.brushes.Add(brush);
        }
    }

    public IReadOnlyCollection<byte[]> GetBrushes()
    {
        return brushes.AsReadOnly();
    }
}
