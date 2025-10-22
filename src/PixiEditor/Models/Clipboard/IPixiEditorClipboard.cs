using Avalonia.Input;

namespace PixiEditor.Models.Clipboard;

public interface IPixiEditorClipboard
{
    Task ClearAsync();
    Task<string> GetTextAsync();
    Task SetTextAsync(string text);
    Task SetDataObjectAsync(IAsyncDataTransfer data);
    Task<T> GetDataAsync<T>(DataFormat<T> id) where T : class;
    Task<IReadOnlyList<DataFormat>> GetFormatsAsync();
}
