using PixiEditor.Extensions.CommonApi.Extensions;
using PixiEditor.Extensions.WasmRuntime;

namespace PixiEditor.Models.ExtensionServices;

public class ExtensionsProvider : IExtensionsProvider
{
    private readonly IExtensionListProvider listProvider;

    public ExtensionsProvider(IExtensionListProvider listProvider)
    {
        this.listProvider = listProvider;
    }

    public string[] GetInstalled()
    {
        return listProvider.LoadedExtensions.Select(x => x.Metadata.UniqueName).ToArray();
    }
}
