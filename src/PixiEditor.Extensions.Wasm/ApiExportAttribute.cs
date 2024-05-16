namespace PixiEditor.Extensions.Wasm;

public class ApiExportAttribute : Attribute
{
    public string ExportName { get; }

    public ApiExportAttribute(string exportName)
    {
        ExportName = exportName;
    }
}
