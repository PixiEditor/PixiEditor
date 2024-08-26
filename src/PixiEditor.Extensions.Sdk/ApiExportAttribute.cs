namespace PixiEditor.Extensions.Sdk;

internal class ApiExportAttribute : Attribute
{
    public string ExportName { get; }

    public ApiExportAttribute(string exportName)
    {
        ExportName = exportName;
    }
}
