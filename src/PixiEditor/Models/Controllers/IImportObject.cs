using Avalonia.Input;
using Avalonia.Input.Platform;

namespace PixiEditor.Models.Controllers;

public interface IImportObject
{
    public bool Contains(string format);
    public Task<object?> GetDataAsync(string format);
}

public class ImportedObject : IImportObject
{
    private readonly IDataObject dataObject;

    public ImportedObject(IDataObject dataObject)
    {
        this.dataObject = dataObject;
    }

    public bool Contains(string format)
    {
        return dataObject.Contains(format);
    }

    public async Task<object?> GetDataAsync(string format)
    {
        return Task.FromResult(dataObject.Get(format));
    }
}

public class ClipboardPromiseObject : IImportObject
{
    public string Format { get; }
    public IClipboard Clipboard { get; }

    public ClipboardPromiseObject(string format, IClipboard clipboard)
    {
        Format = format;
        Clipboard = clipboard;
    }

    public bool Contains(string format)
    {
        return Format == format;
    }

    public async Task<object?> GetDataAsync(string format)
    {
        if (Format != format)
        {
            return null;
        }

        return await Clipboard.GetDataAsync(format);
    }
}
