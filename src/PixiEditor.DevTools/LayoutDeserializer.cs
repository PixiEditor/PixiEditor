using PixiEditor.Extensions.LayoutBuilding;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.DevTools;

public class LayoutDeserializer
{
    private LayoutBuilder _builder;
    public string LayoutFilePath { get; set; }

    public LayoutDeserializer(string layoutFilePath)
    {
        LayoutFilePath = layoutFilePath;
        _builder = new LayoutBuilder(
            (ElementMap)DevToolsExtension.PixiEditorApi.Services.GetService(typeof(ElementMap)));
    }

    public LayoutElement DeserializeLayout()
    {
        List<byte> bytes = new();
        using (FileStream fileStream = new(LayoutFilePath, FileMode.Open))
        {
            byte[] buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                bytes.AddRange(buffer.Take(bytesRead));
            }
        }

        return (LayoutElement)_builder.Deserialize(bytes.ToArray().AsSpan(), DuplicateResolutionTactic.ReplaceRemoveChildren);
    }
}
