using PixiEditor.Extensions.CommonApi.Brushes;

namespace PixiEditor.Extensions.Sdk.Api.Brushes;

public class BrushesDataSource : IBrushDataSource
{
    public string Name { get; }
    private List<byte[]> brushes = new List<byte[]>();

    public BrushesDataSource(string name, IEnumerable<byte[]> brushes)
    {
        Name = name;
        this.brushes.AddRange(brushes);
    }

    public IReadOnlyCollection<byte[]> GetBrushes()
    {
        return brushes.AsReadOnly();
    }
}
