using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;

namespace PixiEditor.Extensions.Runtime;

public static class ExtensionDependencyResolver
{
    /// <summary>
    /// Returns all discovered extensions in dependency order.
    /// Disabled extensions remain in the list, with Disabled = true.
    /// </summary>
    public static List<DiscoveredExtension> ResolveDependencies(List<DiscoveredExtension> discovered)
    {
        var map = discovered.ToDictionary(e => e.Metadata.UniqueName);
        var result = new List<DiscoveredExtension>();

        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var ext in discovered)
        {
            Visit(ext, map, visited, visiting, result);
        }

        return result;
    }
    
    private static void Visit(
        DiscoveredExtension ext,
        Dictionary<string, DiscoveredExtension> map,
        HashSet<string> visited,
        HashSet<string> visiting,
        List<DiscoveredExtension> result)
    {
        if (visited.Contains(ext.Metadata.UniqueName))
            return;

        if (visiting.Contains(ext.Metadata.UniqueName))
        {
            // Circular dependency detected
            ext.Disabled = true;
            return;
        }

        visiting.Add(ext.Metadata.UniqueName);

        foreach (var dep in ext.Metadata.Dependencies ?? [])
        {
            if (!map.TryGetValue(dep, out var dependency))
            {
                // Dependency missing
                ext.Disabled = true;
                continue;
            }

            Visit(dependency, map, visited, visiting, result);

            if (dependency.Disabled)
            {
                ext.Disabled = true;
            }
        }

        visiting.Remove(ext.Metadata.UniqueName);
        visited.Add(ext.Metadata.UniqueName);

        result.Add(ext);
    }
}

