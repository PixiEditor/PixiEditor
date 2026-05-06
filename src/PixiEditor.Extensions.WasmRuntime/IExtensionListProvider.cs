namespace PixiEditor.Extensions.WasmRuntime;

public interface IExtensionListProvider
{
    public IReadOnlyCollection<Extension> LoadedExtensions { get; }
}
