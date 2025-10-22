using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;

namespace PixiEditor.Models.Clipboard;

public class PixiEditorClipboard : IPixiEditorClipboard
{
    private IClipboard avaloniaClipboard;

    public PixiEditorClipboard(IClipboard avaloniaClipboard)
    {
        this.avaloniaClipboard = avaloniaClipboard;
    }

    public Task ClearAsync()
    {
        return avaloniaClipboard.ClearAsync();
    }

    public async Task<string> GetTextAsync()
    {
        return await avaloniaClipboard.TryGetTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        await avaloniaClipboard.SetTextAsync(text);
    }

    public async Task SetDataObjectAsync(IAsyncDataTransfer data)
    {
        avaloniaClipboard.SetDataAsync(data);
    }

    public async Task<T> GetDataAsync<T>(DataFormat<T> id) where T : class
    {
        var data = await avaloniaClipboard.TryGetDataAsync();
        var value = await data.TryGetValueAsync(id);
        data.Dispose();
        return value!;
    }

    public async Task<IReadOnlyList<DataFormat>> GetFormatsAsync()
    {
        return await avaloniaClipboard.GetDataFormatsAsync();
    }

    public async Task<IReadOnlyList<IStorageItem>> GetFilesAsync()
    {
        return await avaloniaClipboard.TryGetFilesAsync();
    }
}
