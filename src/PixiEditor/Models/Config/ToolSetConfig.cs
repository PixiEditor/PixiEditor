using Newtonsoft.Json;

namespace PixiEditor.Models.Config;

public class ToolSetsConfig : List<ToolSetConfig>
{
}

public class ToolSetConfig
{
    public string Name { get; set; }
    public List<string> Tools { get; set; }
}
