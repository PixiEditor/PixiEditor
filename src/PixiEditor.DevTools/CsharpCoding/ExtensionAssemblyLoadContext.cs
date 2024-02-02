using System.Reflection;
using System.Runtime.Loader;

namespace PixiEditor.DevTools.CsharpCoding;

internal class ExtensionAssemblyLoadContext : System.Runtime.Loader.AssemblyLoadContext
{
    public string AssembliesPath { get; set; }
    public ExtensionAssemblyLoadContext(string assembliesPath) : base(true)
    {
        AssembliesPath = assembliesPath;
        Resolving += OnResolving;
    }

    private Assembly? OnResolving(AssemblyLoadContext context, AssemblyName name)
    {
        string? assemblyFileName = $"{name.Name}.dll";
        string? assemblyPath = Path.Combine(AssembliesPath, assemblyFileName);
        if (File.Exists(assemblyPath))
        {
            if (name.Name.StartsWith("PixiEditor"))
            {
                // load from base context
                return null;
            }

            return context.LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }
}
