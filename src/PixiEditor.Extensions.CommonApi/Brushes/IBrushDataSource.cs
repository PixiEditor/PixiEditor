namespace PixiEditor.Extensions.CommonApi.Brushes;

public interface IBrushDataSource
{
    public string Name { get;  }
    public IReadOnlyCollection<byte[]> GetBrushes();
}
