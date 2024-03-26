namespace PixiEditor.Extensions.IO;

public class FileFilter
{
    public List<FileFilterItem> Filters { get; set; } = new();

    public FileFilter()
    {
    }

    public FileFilter AddFilter(string name, params string[] patterns)
    {
        Filters.Add(new FileFilterItem { Name = name, Extensions = patterns.ToList() });
        return this;
    }
}

public class FileFilterItem
{
    public string Name { get; set; } = "";
    public List<string> Extensions { get; set; } = new();
}
