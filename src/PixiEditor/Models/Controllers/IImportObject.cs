using Avalonia.Input;
using Avalonia.Input.Platform;
using PixiEditor.Models.Clipboard;

namespace PixiEditor.Models.Controllers;

public interface IImportObject
{
    public bool Contains<T>(DataFormat<T> format) where T : class;
    public Task<T?> GetDataAsync<T>(DataFormat<T> format) where T : class;
}

public class ImportedObject : IImportObject
{
    private readonly IDataTransfer dataObject;

    public ImportedObject(IDataTransfer dataObject)
    {
        this.dataObject = dataObject;
    }

    public bool Contains<T>(DataFormat<T> format) where T : class
    {
        return dataObject.Contains(format);
    }

    public async Task<T?> GetDataAsync<T>(DataFormat<T> format) where T : class
    {
        return await Task.FromResult(dataObject.TryGetValue<T?>(format));
    }
}

public class ClipboardPromiseObject : IImportObject
{
    public DataFormat Format { get; }
    public IPixiEditorClipboard Clipboard { get; }

    public ClipboardPromiseObject(DataFormat format, IPixiEditorClipboard clipboard)
    {
        Format = format;
        Clipboard = clipboard;
    }

    public bool Contains(DataFormat format)
    {
        return Format == format;
    }

    public async Task<T?> GetDataAsync<T>(DataFormat<T> format) where T : class
    {
        if (Format != format)
        {
            return await Task.FromResult<T?>(default);
        }

        return await Clipboard.GetDataAsync<T>(format);
    }

    public override string ToString()
    {
        return $"ClipboardPromiseObject: {Format}";
    }

    public bool Contains<T>(DataFormat<T> format) where T : class
    {
        return Format == format;
    }
}
