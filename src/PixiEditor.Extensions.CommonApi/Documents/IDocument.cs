namespace PixiEditor.Extensions.CommonApi.Documents;

public interface IDocument
{
    public Guid Id { get; }
    public void Resize(int width, int height);
}
