namespace PixiEditor.OperatingSystem;

public interface ISecureStorage
{
    public Task<T?> GetValueAsync<T>(string key, T? defaultValue = default);
    public Task SetValueAsync<T>(string key, T value);
}
