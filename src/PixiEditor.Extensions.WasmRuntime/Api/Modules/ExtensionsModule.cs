namespace PixiEditor.Extensions.WasmRuntime.Api.Modules;

internal class ExtensionsModule(WasmExtensionInstance extension) : ApiModule(extension)
{
    public IReadOnlyCollection<WasmExtensionInstance> LoadedExtensions
    {
        get
        {
            IExtensionListProvider loader = Extension.Api.Services.GetService(typeof(IExtensionListProvider)) as IExtensionListProvider;
            if (loader == null)
            {
                return [];
            }

            return loader.LoadedExtensions.OfType<WasmExtensionInstance>().ToArray();
        }
    }
}
